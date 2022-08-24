using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mitten
{
    /// <summary>
    /// Interfaccia per le entità che sono costituite da più parti costitutive (i.e.: più bounding box disgiunti).
    /// </summary>
    interface IMultiPart
    {
        /// <summary>
        /// Ottiene la lista delle subentità figlie.
        /// </summary>
        List<SubEntity> getChildren { get; }

        /// <summary>
        /// Ottiene il numero di subentità figlie per l'entità corrente.
        /// </summary>
        int ChildrenNumber { get; }
    }
}
