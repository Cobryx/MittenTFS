using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mitten
{
    public interface ILightEntity
    {
        Krypton.Lights.Light2D Light { get; }
    }
}
