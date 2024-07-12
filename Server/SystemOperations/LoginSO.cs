using Common.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Server.SystemOperations
{
    public class LoginSO : BaseSO
    {

        private readonly MusicProducer musicProducer;
        public LoginSO(LoginValue lval)
        {
            this.musicProducer = lval.Value as MusicProducer;
        }
        protected override void ExecuteConcreteOperation(object parameter = null)
        {
            if (parameter.GetType() == typeof(LoginValue))
                Result = broker.Login((LoginValue)parameter);
        }

    }
}
