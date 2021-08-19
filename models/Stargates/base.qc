// Created by Crowbar 0.71

$modelname "Madman07\stargate\base.mdl"

$bodygroup "body"
{
	studio "base.smd"
}


$surfaceprop "metal"

$contents "solid"

$illumposition 0.009 0 0.064

$cdmaterials "\Madman07\stargate\"

$texturegroup "skinfamilies"
{
	{ "side_sg1"      "inner_sg1"      "base_sg1"      }
	{ "side_infinity" "inner_infinity" "base_infinity" }
}

$cbox 0 0 0 0 0 0

$bbox -8 -140.25 -140.25 10.25 140.25 140.25

$definebone "Bone01" "" 0 0 0 0 -89.999982 0 0 0 0 0 0 0


$sequence "idle" {
	"base_anims\idle.smd"
	fadein 0.2
	fadeout 0.2
	fps 1
}

$collisionmodel "base_physics.smd"
{
	$mass 32000
	$inertia 1
	$damping 0
	$rotdamping 0
	$rootbone " "
	$concave
	$maxconvexpieces 37

}
