@BlockID "PTGNRetractablePDC_Block"
@Version 2
@Author h
@Weaponcore 0

#--- Objects

using Retract as Subpart ("Retract")
using Door1 as Subpart ("Door1")
using Door2 as Subpart ("Door2")
using GatlingTurretBase1 as Subpart ("GatlingTurretBase1") parent Retract
using GatlingTurretBase2 as Subpart ("GatlingTurretBase2") parent GatlingTurretBase1
using emitter as Emitter ("emitter") parent GatlingTurretBase1

var working = false
var extended = true
var PlayerNear = false

#--- Animations

func ResetClosed() {
API.StopDelays()
Door1.reset().rotate([0, 0, 1], -90, 0, Instant).translate([0.073, 0, 0], 0, Instant).translate([0, -1.4, 0], 0, Instant)
Door2.reset().rotate([0, 0, 1], 90, 0, Instant).translate([-0.073, 0, 0], 0, Instant).translate([0, -1.4, 0], 0, Instant)
Retract.reset().translate([0, 1.9, 0], 0, Instant)
}

func ResetOpen() {
API.StopDelays()
Door1.reset()
Door2.reset()
Retract.reset()
}

func KillerBeeMoment() {

if (working == false) {
    if (extended == true) {

       if (PlayerNear == true) {

       API.StopDelays()
       GatlingTurretBase1.movetoorigin(10, Linear)
       GatlingTurretBase2.movetoorigin(10, Linear)
       Retract.reset().delay(30).translate([0, 1.9, 0], 30, Linear)
       Door1.reset().delay(55).rotate([0, 0, 1], -90, 20, Linear).translate([0.073, 0, 0], 4, Linear).delay(4).translate([0, -1.4, 0], 16, Linear)
       Door2.reset().delay(55).rotate([0, 0, 1], 90, 20, Linear).translate([-0.073, 0, 0], 4, Linear).delay(4).translate([0, -1.4, 0], 16, Linear)
       }

       if (PlayerNear == false) {
       ResetClosed()
       }

       extended = false
       }
    }

if (working == true) {
    if (extended == false) {

       if (PlayerNear == true) {

       ResetClosed()
       Retract.delay(15).movetoorigin(30, Linear).delay(30).reset()
       Door1.translate([0, 1.4, 0], 16, Linear).rotate([0, 0, 1], 90, 20, Linear).delay(16).translate([-0.073, 0, 0], 4, Linear).delay(20).reset()
       Door2.translate([0, 1.4, 0], 16, Linear).rotate([0, 0, 1], -90, 20, Linear).delay(16).translate([0.073, 0, 0], 4, Linear).delay(20).reset()
       }

       if (PlayerNear == false) {
       ResetOpen()
       }
       extended = true

       
       }
    }
}



func RecoilCompensatorParticle() {

      emitter.playParticle("EWP_PDCRecoilCompensator", 0.8, 1, [0.0, 0.0, 0.0], 1, 1, 1).delay(5).StopParticle()
      
   }

#--- Actions

action block() {
	
        working() {
                working = true
                KillerBeeMoment()
        }
        notworking() {
                working = false
                emitter.StopParticle()
                KillerBeeMoment()
        }

}

action weaponcore() {
        firing() {
        API.StartLoop("RecoilCompensatorParticle", 5, -1)
        }
        stopfiring() {
        API.StopLoop("RecoilCompensatorParticle")
        emitter.StopParticle()
        }
}

action Distance(1500.0) {
    Arrive() {
    PlayerNear = true
    }
    Leave() {
    PlayerNear = false
    }
}