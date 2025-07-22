@BlockID "MCRNMountedPDCSG_Block"
@Version 2
@Author h
@Weaponcore 0

#--- Objects

using GatlingTurretBase1 as Subpart ("GatlingTurretBase1")
using GatlingTurretBase2 as Subpart ("GatlingTurretBase2") parent GatlingTurretBase1
using emitter as Emitter ("emitter") parent GatlingTurretBase2

var firing = false

#--- Animations

func RecoilCompensatorParticle() {
      if (firing == true) {
      emitter.playParticle("EWP_PDCRecoilCompensator", 0.8, 1, [0.0, 0.0, 0.0], 1, 1, 1).delay(5).StopParticle()
      }

      if (firing == false) {
      emitter.StopParticle()
      }
   }

#--- Actions

action block() {
	
        working() {
                API.StartLoop("RecoilCompensatorParticle", 5, -1)
        }
        notworking() {
                API.StopLoop("RecoilCompensatorParticle")
                emitter.StopParticle()
        }

}

action weaponcore() {
        firing() {
        firing = true
        }
        stopfiring() {
        firing = false
        }
}