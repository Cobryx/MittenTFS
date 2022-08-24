using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Mitten
{

    /// <summary>
    /// Modalità di gioco
    /// </summary>
    enum GameMode
    {
        Practice,
        Timed,
        CaptureTheFlag
    }

    /// <summary>
    /// Proprietà generiche della sessione multiplayer
    /// </summary>
    enum SessionProperty
    {
        GameMode,
        SkillLevel,
        ScoreToWin
    }
    
    /// <summary>
    /// Difficoltà gioco
    /// </summary>
    enum SkillLevel
    {
        Beginner,
        Intermediate,
        Advanced,
        Expert,
        God
    }
}
