using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mitten
{
    public interface IAttacker
    {
        DamageData getDamageDealt { get; }
        int Faction { get; set; }
    }
}
