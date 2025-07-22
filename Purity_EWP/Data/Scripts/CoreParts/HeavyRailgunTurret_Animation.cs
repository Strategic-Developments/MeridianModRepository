using System.Collections.Generic;
using static Scripts.Structure.WeaponDefinition;
using static Scripts.Structure.WeaponDefinition.AnimationDef;
using static Scripts.Structure.WeaponDefinition.AnimationDef.PartAnimationSetDef.EventTriggers;
using static Scripts.Structure.WeaponDefinition.AnimationDef.RelMove.MoveType;
using static Scripts.Structure.WeaponDefinition.AnimationDef.RelMove;
namespace Scripts
{ // Don't edit above this line
    partial class Parts
    {
        private AnimationDef HeavyRailgunTurretAnimation => new AnimationDef
        {

            EventParticles = new Dictionary<PartAnimationSetDef.EventTriggers, EventParticle[]>
            {
                [PreFire] = new[]{
                       new EventParticle
                       {
                           EmptyNames = Names("muzzle_missile_1"),
                           MuzzleNames = Names("muzzle_missile_1"),
                           StartDelay = 0, //ticks 60 = 1 second
                           LoopDelay = 0, //ticks 60 = 1 second
                           ForceStop = false,
                           Particle = new ParticleDef
                           {
                               Name = "MCRNLightRailgun_Prefire",
                               Color = Color(red: 1, green: 1, blue: 1, alpha: 1),
                               Extras = new ParticleOptionDef
                               {
                                   Loop = true,
                                   Restart = true,
                                   MaxDistance = 1000, //meters
                                   MaxDuration = 0, //ticks 60 = 1 second
                                   Scale = 3,
                               }
                           }
                       },
                   },
                [Firing] = new[]{
                       new EventParticle
                       {
                           EmptyNames = Names("muzzle_missile_1"),
                           MuzzleNames = Names("muzzle_missile_1"),
                           StartDelay = 0, //ticks 60 = 1 second
                           LoopDelay = 0, //ticks 60 = 1 second
                           ForceStop = false,
                           Particle = new ParticleDef
                           {
                               Name = "Muzzle_Flash_EWPHeavyRailgunTurret",
                               Color = Color(red: 1, green: 1, blue: 1, alpha: 1),
                               Extras = new ParticleOptionDef
                               {
                                   Loop = true,
                                   Restart = false,
                                   MaxDistance = 2000, //meters
                                   MaxDuration = 10, //ticks 60 = 1 second
                                   Scale = 2,
                               }
                           }
                       },
                   },
            },


 
           };
    }
}
