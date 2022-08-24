using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mitten
{
    public interface IBypass
    {

        List<Waypoint> Alternative
        { get; }
        bool Jumpable
        { get; }
        bool GoDown
        { get; }
        bool Breakable
        { get; }


    }
}
