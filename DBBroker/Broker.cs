using Common.Domain;
using System;
using System.Buffers;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace DBBroker
{
    public class Broker
    {

        private ConnectionBroker connection;
        public Broker()
        {
            connection = new ConnectionBroker();
        }
        public void OpenConnection()
        {
            connection.OpenConnection();
        }
        public void CloseConnection()
        {
            connection.CloseConnection();
        }
        public void Commit()
        {
            connection.Commit();
        }
        public void Rollback()
        {
            connection.Rollback();
        }
        public void BeginTransaction()
        {
            connection.BeginTransaction();
        }
        public object Login(LoginValue loginValue)
        {
            string typeName = loginValue.Type;
            Type entType = Type.GetType(typeName);
            string className = entType.Name;
            string searchQuery = $"SELECT * FROM {className} WHERE ";
            List<string> propertyNames = loginValue.Value.GetType().GetProperties().Select(prop => prop.Name).Where(prop => prop != "TableName" && prop != "Values").ToList();

            // Provera da li je Parameter definisan i ima vrednosti
            if (loginValue.Parameter != null && loginValue.Parameter.Length > 0)
            {
                // Dodavanje uslova za svaki parametar
                for (int i = 0; i < loginValue.Parameter.Length; i++)
                {
                    string paramName = loginValue.Parameter[i];
                    object paramValue = null;
                    if (loginValue.Value is MusicProducer)
                    {
                        MusicProducer musicProducer = (MusicProducer)loginValue.Value;

                        switch (paramName)
                        {
                            case "Username":
                                paramValue = musicProducer.Username;
                                break;
                            case "Password":
                                paramValue = musicProducer.Password;
                                break;
                            default:
                                paramValue = null;
                                break;
                        }
                    }
                    if (paramValue != null)
                    {
                        searchQuery += $"{paramName} = @{paramName}";
                        if (i < loginValue.Parameter.Length - 1)
                            searchQuery += " AND ";
                    }
                }

                // Priprema SQL komande
                SqlCommand cmd = connection.CreateCommand();
                for (int i = 0; i < loginValue.Parameter.Length; i++)
                {
                    string paramName = loginValue.Parameter[i];
                    object paramValue = null;

                    // Dobijamo vrednost atributa objekta MusicProducer
                    if (loginValue.Value is MusicProducer)
                    {
                        MusicProducer musicProducer = (MusicProducer)loginValue.Value;

                        switch (paramName)
                        {
                            case "Username":
                                paramValue = musicProducer.Username;
                                break;
                            case "Password":
                                paramValue = musicProducer.Password;
                                break;
                            default:
                                paramValue = null;
                                break;
                        }
                    }
                    if (paramValue != null)
                    {
                        cmd.Parameters.AddWithValue($"@{paramName}", paramValue);
                    }
                    else
                    {
                        return null;
                    }

                }
                cmd.CommandText = searchQuery;
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    // Kreiranje instance objekta na osnovu tipa
                    Type entityType = Type.GetType(loginValue.Type);
                    ConstructorInfo constructor = entityType.GetConstructor(Type.EmptyTypes);
                    IEntity obj = (IEntity)constructor.Invoke(null);

                    // Postavljanje vrednosti svojstava objekta iz rezultata upita
                    foreach (var property in obj.GetType().GetProperties())
                    {
                        if (propertyNames.Contains(property.Name))
                        {
                            var value = reader[property.Name];
                            if (value != DBNull.Value)
                            {
                                property.SetValue(obj, value);
                            }
                        }
                    }

                    // Vraćanje objekta koji odgovara pronađenom redu
                    cmd.Dispose();
                    reader.Close();
                    return obj;
                }
                // Ako ne pronađemo odgovarajući red, vraćamo null
                reader.Close();
                cmd.Dispose();
                return null;
            }

            return null;
        }        
        public object Add(IEntity obj)
        {
            SqlCommand cmd = connection.CreateCommand();
            List<string> propertyNames = obj.GetType().GetProperties()
                .Where(prop => prop.Name != "TableName" && prop.Name != "Values" && prop.Name != "Id")
                .Select(prop => prop.Name)
                .ToList();

            string sourcePart = string.Join(",", propertyNames);
            string values = obj.Values;

            List<string> onPartList = propertyNames.
                Where(prop => prop != "CreationDate" && prop != "Artist" && prop != "MusicProducer" && prop != "Director" && prop != "Song" && prop != "MusicVideo" && prop != "Project")
                .Select(key => $"target.{key} = source.{key} and").ToList();
            string onPart = string.Join(" ", onPartList);
            onPart = onPart.Remove(onPart.LastIndexOf("and"), 3);

            cmd.CommandText = $"MERGE INTO {obj.TableName} AS target USING (VALUES ({values})) AS source ({sourcePart})" +
                              $" ON {onPart} WHEN NOT MATCHED THEN INSERT ({sourcePart}) VALUES ({values});";

            int rowsAffected = cmd.ExecuteNonQuery();
            cmd.Dispose();
            return rowsAffected == 1 ? obj : null;
        }       
        private List<string> GetAllPrimaryKeys(string tableName = "")
        {
            string tableQuery = string.IsNullOrEmpty(tableName) ? "" : $"and table_name = '{tableName}'";
            List<string> primaryKeys = new List<string>();
            SqlCommand cmd = connection.CreateCommand();
            cmd.CommandText = $"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE WHERE OBJECTPROPERTY(OBJECT_ID(CONSTRAINT_SCHEMA + '.' + CONSTRAINT_NAME), 'IsPrimaryKey') = 1 {tableQuery};";
            SqlDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                primaryKeys.Add(reader[0].ToString());
            }
            return primaryKeys;
        }
        public string GetUpdateQuery(EditValue editValue)
        {
            string setQuery = "set ";
            Type type = Type.GetType(editValue.Type);
            List<PropertyInfo> properties = type.GetProperties().Where(prop => prop.Name != "TableName" && prop.Name != "Values").ToList();
            List<string> primaryKeys = GetAllPrimaryKeys(type.Name);
            foreach (var property in properties)
            {
                if (primaryKeys.Contains(property.Name)) 
                    continue;
                object setValue = property.GetValue(editValue.EditedValue);
                if (setValue == null)
                {
                    setQuery += $"{property.Name} = NULL,";
                    continue;
                }
                //ako je property slozen, odnosno predstavlja domensku klasu
                if (setValue!=null && property.PropertyType.GetInterfaces().ToList().Contains(typeof(IEntity)))
                {
                    string innerPrimaryKey = GetAllPrimaryKeys(property.PropertyType.Name)[0];
                    PropertyInfo innerProperty = property.PropertyType.GetProperty(innerPrimaryKey);
                    setValue = innerProperty.GetValue(setValue);
                }
                //ako je datum pretvori ga u string odg formata koji sql prepoznaje
                if (setValue.GetType() == typeof(DateTime))
                    setValue = ((DateTime)setValue).ToString("yyyy-MM-dd HH:mm:ss");
                
                //ako je setValue string samo mu dodaj navodnike da bi ga sql prepoznao kao string
                if (setValue.GetType().IsEnum)
                    setValue = setValue.ToString();
                //ako je string dodaj mu ''
                setValue = setValue.GetType() == typeof(string) ?$" '{setValue}' " : setValue;
                setQuery += $"{property.Name} = {setValue},";
            }
            //brisemo , sa kraja
            setQuery = setQuery.TrimEnd(',');
            string whereQuery = "  where ";
            Console.WriteLine(setQuery);
            foreach (var primaryKey in primaryKeys)
            {
                PropertyInfo idProperty = type.GetProperty(primaryKey);
                var primaryKeyValue = idProperty.GetValue(editValue.OriginalValue);
                // ako je string dodaj mu ''
                primaryKeyValue = primaryKeyValue.GetType() == typeof(string) ? $" '{primaryKeyValue}' " : primaryKeyValue;
                whereQuery += $"{primaryKey} = {primaryKeyValue} and";
            }
            //brisi and sa kraja
            whereQuery = whereQuery.Remove(whereQuery.LastIndexOf("and"), 3);
            Console.WriteLine($"update {type.Name} {setQuery} {whereQuery}");
            return $"update {type.Name} {setQuery} {whereQuery}";
        }
        private bool IsListOfIEntity(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                Type[] genericArguments = type.GetGenericArguments();
                if (genericArguments.Length == 1 && typeof(IEntity).IsAssignableFrom(genericArguments[0]))
                {
                    return true;
                }
            }
            return false;
        }
        public object Edit(EditValue editValue)
        {
            Type editedValueType = editValue.EditedValue.GetType();
            Type originalValueType = editValue.OriginalValue.GetType();
            int affected = 0;
            //for petlja
            if (IsListOfIEntity(editedValueType) && IsListOfIEntity(originalValueType))
            {   
                IList editedEntities = (IList)editValue.EditedValue;
                IList originalEntities = (IList)editValue.OriginalValue;
                for(int i =0; i < editedEntities.Count; i++)
                {
                    object editedEntity = editedEntities[i];
                    object originalEntity = originalEntities[i];
                    Type entityType = editedEntity.GetType();
                    EditValue newEV = new EditValue();
                    newEV.EditedValue = editedEntities[i];
                    newEV.OriginalValue = originalEntities[i];
                    newEV.Type = entityType.AssemblyQualifiedName;

                    SqlCommand cmd = connection.CreateCommand();
                    cmd.CommandText = GetUpdateQuery(newEV);
                    affected+= cmd.ExecuteNonQuery();
                    cmd.Dispose();
                }          
            }
            else
            {
                SqlCommand cmd = connection.CreateCommand();
                cmd.CommandText = GetUpdateQuery(editValue);
                affected+= cmd.ExecuteNonQuery();
                cmd.Dispose();
            }
            
            if (affected == 0)
                return null;
            else
                return editValue;
        }     
        public object Search(SearchValue searchValue)
        {
            //za sad radi search samo po jednom parametru, dobro bi bilo da se osposobi da gleda za svaku klasu vise parametara
            string searchQuery;
            //pravi se konstruktor klase(tabele) u kojoj pretrazujemo, npr passenger
            Type type =  Type.GetType(searchValue.Type);

            ConstructorInfo constructor = type.GetConstructor(Type.EmptyTypes);
            //inicijalizacija objekta te klase 
            IEntity obj = (IEntity)constructor.Invoke(null);
            //nazivi kolona u tabeli   
            List<string> propertyNames = obj.GetType().GetProperties().Select(prop => prop.Name).Where(prop => prop != "TableName" && prop != "Values").ToList();
            //lsita objekata u koju smestamo vrednosti izvucene iz tabele
            BindingList<object> objects = new BindingList<object>();
            SqlCommand cmd = connection.CreateCommand();

            //kada se forma ucita prvi put prikazace se svi objekti u bazi, odnosno nema where query-ja

            if (string.IsNullOrEmpty((string)searchValue.Value.ToString()))
                searchQuery = string.Empty;
            else
                if (searchValue.Parameter == "Id" || searchValue.Parameter == "Project" 
                || searchValue.Parameter == "MusicVideo" || searchValue.Parameter == "Song" 
                || searchValue.Parameter == "Artist" || searchValue.Parameter == "MusicProducer"
                || searchValue.Parameter == "Director")
                    searchQuery = $"where {searchValue.Parameter} like '{searchValue.Value}' ";
                else
                    searchQuery = $"where {searchValue.Parameter} like '%{searchValue.Value}%' ";
            

            cmd.CommandText = $"select * from {obj.TableName} " + $"{searchQuery}";
            SqlDataReader reader = cmd.ExecuteReader();

            //ucitan red iz tabele
            while (reader.Read())
            {
                obj = (IEntity)constructor.Invoke(null);
                //za svaku kolonu iz liste kolona te tabele postavi vrednost u property tog objekta
                foreach (string column in propertyNames)
                {
                    //property objekta u koji zelimo da upisemo odg vrednost dobijamo ovako
                    PropertyInfo property = obj.GetType().GetProperty(column);
                    Type interfaceType = typeof(IEntity);
                    /*Ako naidjemo na spoljni kljuc odnosno ako je naziv kolone ujedno i naziv domenske klase(to cemo znati ako nasledjuje IEntity),
                      ne mozemo u njega da ubacimo int vrednost nego moramo da napravimo objekat te klase
                      i da iz njegove tabele izvucemo objekat sa odg ID-jem */
                    if (property.PropertyType.GetInterfaces().Contains(interfaceType))
                    {
                        ConstructorInfo innerConstructor = property.PropertyType.GetConstructor(Type.EmptyTypes);
                        object innerObj = innerConstructor.Invoke(null);
                        List<string> innerObjColumns = innerObj.GetType().GetProperties().Select(prop => prop.Name).Where(prop => prop != "TableName" && prop != "Values").ToList();
                        SearchValue innerSearchValue = new SearchValue()
                        {
                            Type = property.PropertyType.AssemblyQualifiedName,
                            Parameter = "Id",
                            Value = reader[column],
                        };

                        BindingList<object> innerList = Search(innerSearchValue) as BindingList<object>;
                        if(innerList != null && innerList.Count == 1)
                        {
                            object firstElement = innerList[0];
                            Type valueType = property.PropertyType;
                            if (valueType.IsEnum)
                                firstElement = Enum.Parse(valueType, (string)firstElement);
                            property.SetValue(obj, firstElement);
                        }
                        else
                        {
                            property.SetValue(obj, null);
                        }


                    }
                    else
                    {
                        object value = reader[column];
                        Type valueType = property.PropertyType;
                        if (valueType.IsEnum) value = Enum.Parse(valueType, (string)value);
                        property.SetValue(obj, value);
                    }
                }
                objects.Add(obj);
            }
            reader.Close();
            return objects;
        }   
    }
}
