using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace Mitten
{
    /// <summary>
    /// Stato di attività di un'entità
    /// </summary>
    enum activityCode
    {
        freeze,
        hibernate,
        hide,
        reistantiate,
        reboot,
        reset,
        suspend,
        unfreeze,
        wait
    }

    /// <summary>
    /// Tipo di collisione
    /// </summary>
    enum collisionType
    {
        touch,
        mechanical,
        explosion,
        thrust,
        blunt,
        wound,
        contact,
        ethereal




    }

    /// <summary>
    /// Tipi di effetti
    /// </summary>
    enum damageEffects
    {
        burn,
        freeze,
        stun,
        critic,
        petrify,
        slow,
        curse,
        blind,
        block,
        mechanical,
        poison,
        hot,    //healing over time
        buff,
        debuff,
        fear,
        illness



    }

    /// <summary>
    /// Tipi di danno (per il calcolo del danno totale sulla base dei modificatori, etc)
    /// </summary>
    enum damageTypes
    {
        physical,
        fire,
        water,
        air,
        earth,
        energy,
        poison,
        spectral
    }

    /// <summary>
    /// Possibili cause di morte.
    /// </summary>
    enum deathCauses
    {
        generic,
        bleeding,
        fire
    }

    /// <summary>
    /// Stili del dungeon (colori per le mattonelle)
    /// </summary>
    enum tileColors
    {
        green,
        black,
        red,
        gray,
        chess,
        arlequin,
        mage_quarter,
        cave,
        temple
    }

    /// <summary>
    /// Tipi di unità (corrispondenti a classi che implementano l'interfaccia IEntity e a relative sottoclassi o istanze particolari).
    /// </summary>
    enum entityTypes               //tipi di entità, da completare pian piano che il progetto va avanti
    {
        human,
        missile,
        door,
        table,
        explosion,
        dead,
        scenery,
        trigger,
        magic,
        zombie,
        spiderbot,
        wizard,
        banshee,
        item,
        torch,
        stairs,
        throwable,
        magicbolt,
        simplescenography,
        icewall,
        altar,
        anvil,
        bigbook,
        column,
        charredwood,
        columnbasement1,
        columnbasement2,
        columnbasement3,
        gargoyle,
        hammer,
        pot1,
        pot2,
        throne1,
        throne2,
        bloodfountain,
        corpse1,
        corpse2,
        corpse3,
        corpsestack,
        head,
        manhole1,
        manhole2,
        manhole3,
        sleepbag1,
        sleepbag2
    }

    /// <summary>
    /// Tipi di unità (corrispondenti a classi che implementano l'interfaccia IEntity e a relative sottoclassi o istanze particolari).
    /// </summary>
    enum sheetIndexes               //tipi di entità, da completare pian piano che il progetto va avanti
    {
        human1,
        human2,
        human3,
        human4,
        door,
        table,
        explosion,
        //trigger,
        zombie1,
        zombie2,
        zombie3,
        spiderbot,
        wizard1,
        wizard2,
        wizard3,
        wizard4,
        wizard5,
        wizard6,
        wizard7,
        wizard8,
        banshee1,
        banshee2,
        banshee3,
        item,
        torch,
        stairs,
        magicbolt,
        simplescenography,
        wall,
        blaze,
        laser,
        magictrail,
        GUIheart,
        orb,
        icewall,
        altar,
        altardecoration,
        anvil,
        bigbook,
        column,
        charredwood,
        columnbasements1,
        columnbasements2,
        columnbasements3,
        gargoyle,
        hammer,
        pot1,
        pot2,
        throne1,
        throne2,
        bloodfountain,
        corpse1,
        corpse2,
        corpse3,
        corpsestack,
        head,
        manhole1,
        manhole2,
        manhole3,
        sleepbag1,
        sleepbag2
    }

    /// <summary>
    /// Indici per l'array degli oggetti equipaggiati
    /// </summary>
    enum equipSlots
    {
        shield,
        armor,
        belt,
        amulet,
        ring,
        //ring2,
        melee,
        ranged,
        ammoBow,
        ammoCrossbow,
        throwing
    }

    /// <summary>
    /// Id unici per l'appartanenza delle enetità
    /// </summary>
    enum factions
    {
        foes=32768,//Globals.max_entities,
        ambient,
        traps,
        explosions
    }

    /// <summary>
    /// Categorie di oggetti impugnabili per le classi Ihandlebles
    /// </summary>
    enum handleables
    {
        bow,
        crossbow,
        mace,
        nothing,
        rock,
        sword












    }

    /// <summary>
    /// Abilità del giocatore.
    /// </summary>
    enum skills
    {
        none = -1,
        firebolt,
        laser,
        shield,
        icewall,
        fireball,
        firewall,
        blaze,
        icespike,
        freeze,
        water,
        golem,
        stoneskin,
        wave,
        lightning,
        light,
        wind,
        energyshield,
        buffs,
        subfirewall
    }

    /// <summary>
    /// Stati della classe banshee
    /// </summary>
    enum b_states
    {
        dying,
        dead,
        idle,
        attacking,
        walking,
        rotating,
        delayed

    }

    /// <summary>
    /// Animazioni della classe banshee
    /// </summary>
    enum b_animations
    {
        dying,
        dead,
        idle,
        walking,
        attacking,
        



    }

    /// <summary>
    /// Animazioni della classe Door.
    /// </summary>
    enum dr_animations
    {
        idle,
        open,
        closed,
        broken



    }

    /// <summary>
    /// Stati della classe Door.
    /// </summary>
    enum dr_states
    {
        idle,
        open,
        closed,
        broken



    }

    /// <summary>
    /// Animazioni della classe explosion
    /// </summary>
    enum ex_animations
    {
        expanding,
        collapsing,
        exhausted,
        fallout



    }

    /// <summary>
    /// Tipi di explosions
    /// </summary>
    enum explosion_types
    {
        gas,
        typical,
        napalm



    }

    /// <summary>
    /// Stati di explosion
    /// </summary>
    enum ex_states
    {
        expanding,
        collapsing,
        exhausted,
        fallout



    }

    /// <summary>
    /// Stile del tileset dei pavimenti per un dato dungeon
    /// </summary>
    enum floorTypes
    {
        marble,
        cave,
        parquet,
        stone
    }

    /// <summary>
    /// Animazioni della classe Human.
    /// </summary>
    enum h_animations
    {
        dying,
        dead,
        idle,
        idle2,
        idle3,
        walking,
        running,
        jumping,
        attacking,
        attacking2,
        attacking3,
        parrying,
        throwing,
        shooting,
        floating,
        magic1,
        magic2,
        magic3,
        kicking,
        punching,
        throwing2,
        
    }

    /// <summary>
    /// Indici dei boundbox collisori della classe Human.
    /// </summary>
 
    /// <summary>
    /// Stati della classe Human.
    /// </summary>
    enum h_states
    {
        dying,      //blocca tutto e permette solo la morte
        dead,       //blocca tutto e disattiva l'entità stessa
        idle,       //non blocca niente
        walking,    //non blocca niente ma esclude running
        running,    //non blocca niente ma esclude walking
        jumping,    //blocca tutto tranne dying, rotating, floating e falling
        falling,    //blocca tutto tranne dying, rotating e floating
        
        attacking,  //interrompe corsa, parata e magia, blocca throwing e shooting
        parrying,   //interrompe corsa, attacco e magia, blocca tutto tranne dying, rotating, floating e falling
        throwing,   //interrompe la corsa, blocca shooting, attacking, magic e parrying
        shooting,   //interrompe la corsa, blocca throwing, attacking, magic e parrying
        floating,   //blocca tutto tranne dying e rotating
        magic,      //interrompe la corsa, blocca attacking, parrying, throwing e shooting
        
        kicking,    //blocca tutto tranne dying e floating
        rotating,   //non blocca niente ma interrompe idle
        punching,   //blocca tutto tranne dying e floating
        action,     //blocca attacking, shooting, throwing, parrying e magic
        
    }

    enum projectileType
    {
        linear,
        parabolic,
        rotational, //non lo implementeremo MAI
        enemyseeking
    }

    /// <summary>
    /// Indice degli effetti sonori
    /// </summary>
    enum sounds
    {
        b_attack,
        b_idle,
        b_walk,
        b_ouch,
        b_death,
        e_fall,
        e_splat,
        eff_burn,
        eff_ice,
        eff_energy,
        gui_select,
        gui_select2,
        gui_menu,
        gui_chat,
        p_drink,
        p_equip,
        p_ouch1,
        p_ouch2,
        p_ouch3,
        p_swear1,
        p_swear2,
        p_attack1,
        p_attack2,
        r_boom1,
        r_boom2,
        r_broken_glass,
        r_broken_wood,
        r_broken_bone,
        r_broken_stone,
        r_metal1,
        r_metal2,
        r_metal3,
        r_bowstring1,
        r_bowstring2,
        r_drop, //...
        r_glass,
        r_wood,
        r_bone,
        sk_,
        sp_attack,
        sp_idle,
        sp_walk,
        sp_ouch,
        sp_death,
        w_attack,
        w_idle,
        w_walk,
        w_ouch,
        w_death,
        w_teleport1,
        w_teleport2,
        z_attack,
        z_idle,
        z_walk,
        z_ouch,
        z_death,
    }

    enum songs
    {
        environment1,
        environment2,
        environment3,
        environment4
    }

    /// <summary>
    /// Stati della classe spiderbot
    /// </summary>
    enum sp_states
    {
        dying,
        dead,
        idle,
        attacking,
        walking,
        rotating,
        delayed



    }

    /// <summary>
    /// Animazioni della classe spiderbot
    /// </summary>
    enum sp_animations
    {
        dying,
        dead,
        idle,
        walking,
        attacking,
        



    }

    enum th_animations
    {
        idle,
        launched,
        landing,
        inhert,
        stuck
    }
    enum th_states
    {
        idle,
        launched,
        landing,
        inhert,
        stuck
    }

    /// <summary>
    /// Animazioni della classe Table.
    /// </summary>
    enum tb_animations
    {
        idle,
        flipping,
        flipped100,
        flipped80,
        flipped60,
        flipped40,
        flipped20,
        broken,
        breaked
    }

    /// <summary>
    /// Stati della classe Table.
    /// </summary>
    enum tb_states
    {
        idle,
        flipping,
        flipped,
        broken,
        breaked
    }

    /// <summary>
    /// Stile del tileset per i muri di un dato dungeon.
    /// </summary>
    enum wallTypes
    {
        standard,
        alternative,
        rock,
        wood,
        greek
    }

    /// <summary>
    /// Stati della classe wizard
    /// </summary>
    enum w_states
    {
        dying,
        dead,
        idle,
        attacking,
        walking,
        rotating,
        teleport,
        deteleport



    }

    /// <summary>
    /// Animazioni della classe wizard
    /// </summary>
    enum w_animations
    {
        dying,
        dead,
        idle,
        walking,
        attacking,
        teleport,
        deteleport
        
    }

    /// <summary>
    /// Stati della classe zombie
    /// </summary>
    enum z_states
    {
        dying,
        dead,
        idle,
        walking,
        attacking,
        rotating,
        delayed



    }

    /// <summary>
    /// Animazioni della classe zombie
    /// </summary>
    enum z_animations
    {
        dying,
        dead,
        idle,
        walking,
        attacking,
        
    }

    /// <summary>
    /// Bottoni del gamepad generico (non XBOX 360)
    /// </summary>
    enum xgbuttons
    {
        A=1,
        B=2,
        LeftThumbPress=10,
        LeftTrigger=6,
        RightTrigger=7,
        RightShoulder=5,
        LeftShoulder=4,
        DPadRight=999,
        DPadLeft=999,
        DPadDown=999,
        DPadUp=999,
        X=0,
        Y=3,
        RightThumbPress=11,
        Select=8,
        Start=9,
        Home=12,
    }

}
