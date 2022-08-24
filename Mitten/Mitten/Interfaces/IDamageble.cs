using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mitten
{
    public interface IDamageble
    {
        void SetDamageData(DamageData a);
        
        int Faction { get; set; }
        DamageManager DamageManager{get;set;}
        bool Alive { get; }
    }
}
