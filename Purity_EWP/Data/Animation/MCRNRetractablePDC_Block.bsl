@BlockID "MCRNRetractablePDC_Block"
@Version 2
@Author h
@Weaponcore 0

#--- Objects

using Elevator as Subpart ("Elevator")
using Door1 as Subpart ("Door1")
using Door1Corner1 as Subpart ("Door1Corner1") parent Door1
using Door1Corner2 as Subpart ("Door1Corner2") parent Door1
using Door2 as Subpart ("Door2")
using Door2Corner1 as Subpart ("Door2Corner1") parent Door2
using Door2Corner2 as Subpart ("Door2Corner2") parent Door2

using GatlingTurretBase1 as Subpart ("GatlingTurretBase1") parent Elevator

using turrethinge as Subpart ("turrethinge") parent GatlingTurretBase1
using turretpiston as Subpart ("turretpiston") parent GatlingTurretBase1
using turretpistonExtension as Subpart ("turretpistonExtension") parent turretpiston

using GatlingTurretBase2 as Subpart ("GatlingTurretBase2") parent turrethinge

using emitter as Emitter ("emitter") parent GatlingTurretBase2

var working = false
var extended = true
var firing = false
var PlayerNear = false

#--- Animations

func ResetClosed() {
       API.StopDelays()
       GatlingTurretBase1.reset()
       GatlingTurretBase2.reset()
       Elevator.reset().translate([0, 1.9, 0], 0, Instant)
       Door1.reset().translate([0, -0.795, 0], 0, Instant).rotate([0, 0, 1], -90, 0, Instant)
       Door1Corner1.reset().rotate([0, 1, 0], -180, 0, Instant)
       Door1Corner2.reset().rotate([0, 1, 0], 180, 0, Instant)
       Door2.reset().translate([0, -0.795, 0], 0, Instant).rotate([0, 0, 1], 90, 0, Instant)
       Door2Corner1.reset().rotate([0, 1, 0], -180, 0, Instant)
       Door2Corner2.reset().rotate([0, 1, 0], 180, 0, Instant)
       turrethinge.reset().rotate([0, 0, 1], -90, 0, Instant).translate([0, 1.6, 0], 0, Instant)
       turretpiston.reset().translate([0, 1.6, 0], 0, Instant)
       turretpistonExtension.reset().translate([0, -0.8, 0], 0, Instant)
}

func ResetOpen() {
       API.StopDelays()
       GatlingTurretBase1.reset()
       GatlingTurretBase2.reset()
       Elevator.reset()
       Door1.reset()
       Door1Corner1.reset()
       Door1Corner2.reset()
       Door2.reset()
       Door2Corner1.reset()
       Door2Corner2.reset()
       turrethinge.reset()
       turretpiston.reset()
       turretpistonExtension.reset()
}

func KillerBeeMoment() {

if (working == false) {
    if (extended == true) {

       if (PlayerNear == true) {

       API.StopDelays()
       GatlingTurretBase1.movetoorigin(20, Linear)
       GatlingTurretBase2.movetoorigin(20, Linear)
       Elevator.reset().delay(20).translate([0, 1.9, 0], 30, Linear)
       Door1.reset().delay(20).translate([0, -0.795, 0], 20, Linear).delay(20).rotate([0, 0, 1], -90, 20, Linear)
       Door1Corner1.reset().delay(40).rotate([0, 1, 0], -180, 20, Linear)
       Door1Corner2.reset().delay(40).rotate([0, 1, 0], 180, 20, Linear)
       Door2.reset().delay(20).translate([0, -0.795, 0], 20, Linear).delay(20).rotate([0, 0, 1], 90, 20, Linear)
       Door2Corner1.reset().delay(40).rotate([0, 1, 0], -180, 20, Linear)
       Door2Corner2.reset().delay(40).rotate([0, 1, 0], 180, 20, Linear)

       turrethinge.reset().rotate([0, 0, 1], -90, 15, Linear).translate([0, 1.6, 0], 20, Linear)
       turretpiston.reset().translate([0, 1.6, 0], 20, Linear)
       turretpistonExtension.reset().translate([0, -0.8, 0], 20, Linear)

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
       Elevator.delay(15).translate([0, -1.9, 0], 30, Linear).delay(30).reset()
       Door1.rotate([0, 0, 1], 90, 20, Linear).delay(20).translate([0, 0.795, 0], 20, Linear).delay(20).reset()
       Door1Corner1.rotate([0, 1, 0], 180, 20, Linear).delay(20).reset()
       Door1Corner2.rotate([0, 1, 0], -180, 20, Linear).delay(20).reset()
       Door2.rotate([0, 0, 1], -90, 20, Linear).delay(20).translate([0, 0.795, 0], 20, Linear).delay(20).reset()
       Door2Corner1.rotate([0, 1, 0], 180, 20, Linear).delay(20).reset()
       Door2Corner2.rotate([0, 1, 0], -180, 20, Linear).delay(20).reset()

       turrethinge.delay(30).translate([0, -1.6, 0], 20, Linear).delay(10).rotate([0, 0, 1], 90, 15, Linear).delay(15).reset()
       turretpiston.delay(30).translate([0, -1.6, 0], 20, Linear).delay(25).reset()
       turretpistonExtension.delay(30).translate([0, 0.8, 0], 20, Linear).delay(25).reset()

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