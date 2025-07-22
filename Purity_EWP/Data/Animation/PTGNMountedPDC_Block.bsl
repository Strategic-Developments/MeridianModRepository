@BlockID "PTGNMountedPDC_Block"
@Version 2
@Author h
@Weaponcore 0

#--- Objects

using GatlingTurretBase1 as Subpart ("GatlingTurretBase1")
using emitter as Emitter ("emitter") parent GatlingTurretBase1

#--- Animations

func RecoilCompensatorParticle() {

      emitter.playParticle("EWP_PDCRecoilCompensator", 0.8, 1, [0.0, 0.0, 0.0], 1, 1, 1).delay(5).StopParticle()
      
   }

#--- Actions

action block() {
	
        notworking() {
                emitter.StopParticle()
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