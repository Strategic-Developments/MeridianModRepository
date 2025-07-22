@BlockID "Aryx_AES_6M40_PDC"
@Version 2
@Author AryxCami
@Weaponcore 0

#--- Declarations
using platform as Subpart("platform")
using azimuth as Subpart("azimuth") parent platform
using strut as Subpart("strut") parent azimuth
using rotor as Subpart("rotor") parent azimuth
using zenith as Subpart("zenith") parent rotor

using doorLeft as Subpart("door_left")
using doorRight as Subpart("door_right")
using doorFlapL1 as Subpart("panel_1") parent doorLeft 
using doorFlapL2 as Subpart("panel_2") parent doorLeft 

using doorFlapR1 as Subpart("panel_1") parent doorRight
using doorFlapR2 as Subpart("panel_2") parent doorRight

using light as light("light_1", 2.5)

using sound as emitter("light_1")

var bIsDeployed = true

func Retract() {


	sound.playsound("AES_PDC_ND6M40_DeployClose")
	
	#--- Doors
	doorFlapL1.delay(50).rotate([1, 0, 0], 30, 15, Linear)
	doorFlapL2.delay(50).rotate([1, 0, 0], -30, 15, Linear)		

	doorFlapR1.delay(50).rotate([1, 0, 0], 30, 15, Linear)
	doorFlapR2.delay(50).rotate([1, 0, 0], -30, 15, Linear)	
	
	doorLeft.delay(50).rotate([0, 0, 1], 90, 15, InOutSine )
	doorRight.delay(50).rotate([0, 0, 1], -90, 15, InOutSine )

	doorLeft.delay(10).translate([0.02, -1.15, 0], 30, Linear)
	doorRight.delay(10).translate([-0.02, -1.15, 0], 30, Linear)

	
	#--- Platform
	platform.delay(30).translate([0, 1.75, 0], 30, InBack)
	
	strut.delay(5).translate([0, 3.2207, 0], 30, InOutSine)
	strut.delay(25).setvisible(false)
	
	rotor.delay(5).translate([0, 3.1902, 0], 30, InOutSine)	
	rotor.rotate([0, 0, 1], -90, 25, InOutSine)	
		
	light.lightoff()		
}

func Deploy() {
	sound.playsound("AES_PDC_ND6M40_DeployOpen")
	light.setcolor(25, 127, 255)
	light.lighton()	

	#--- Doors
	
	doorFlapL1.rotate([1, 0, 0], -30, 15, Linear)
	doorFlapL2.rotate([1, 0, 0], 30, 15, Linear)		

	doorFlapR1.rotate([1, 0, 0], -30, 15, Linear)
	doorFlapR2.rotate([1, 0, 0], 30, 15, Linear)		

	doorLeft.delay(10).translate([-0.02, 1.15, 0], 40, Linear)
	doorRight.delay(10).translate([0.02, 1.15, 0], 40, Linear)
	
	doorLeft.rotate([0, 0, 1], -90, 15, InOutSine )
	doorRight.rotate([0, 0, 1], 90, 15, InOutSine )	


	#--- Platform
	
	rotor.delay(35).rotate([0, 0, 1], 90, 20, InOutSine)		
	rotor.delay(25).translate([0, -3.1902, 0], 30, InOutSine  )	
	strut.delay(30).setvisible(true)	
	strut.delay(25).translate([0, -3.2207, 0], 30, InOutSine  )
	
	platform.delay(10).translate([0, -1.75, 0], 30, OutBack)	
	
	
}

#--- Actions

action block() {
	Working() {
		if (bIsDeployed == false){
			light.setcolor(25, 127, 255)
			light.lighton()		
			Deploy()
			bIsDeployed = true
		}
	}
	
	Notworking() {	
		if (bIsDeployed == true){
			bIsDeployed = false
			Retract()
			light.lightoff()	
		}
	}
}
