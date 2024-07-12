using Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.SystemOperations
{
    public class AddMusicVideoSO : BaseSO
    {
        protected override void ExecuteConcreteOperation(object parameter = null)
        {
            Type interfaceType = typeof(IEntity);
            if (parameter.GetType().GetInterfaces().Contains(interfaceType))
                Result = broker.Add((IEntity)parameter);
        }
    }
}
