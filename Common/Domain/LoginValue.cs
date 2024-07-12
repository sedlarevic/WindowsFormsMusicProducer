﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Domain
{
    [Serializable]
    public class LoginValue
    {

        public string Type { get; set; } = typeof(MusicProducer).AssemblyQualifiedName;

        public string[] Parameter { get; set; } = new string[] { "Username", "Password" };

        //vrednost iz textboxa po kojoj pretrazujemo
        public object Value { get; set; }

    }
}
