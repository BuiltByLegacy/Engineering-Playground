class_name EngineeringLearnCatalog
extends RefCounted

var _concepts := {
	"flow_rate": {"title":"Flow Rate","explorer":"How much fluid gets through over time.","engineer":"Volumetric flow rate measures fluid volume crossing a section per unit time."},
	"pressure": {"title":"Pressure","explorer":"How hard the fluid is pushing.","engineer":"Pressure is force per unit area and drives flow through a system."},
	"velocity": {"title":"Velocity","explorer":"How fast the fluid is moving.","engineer":"Velocity is the local speed and direction of the fluid field."},
	"restriction": {"title":"Restriction","explorer":"Tight paths make flow work harder.","engineer":"Restrictions increase losses and often raise local velocity and pressure drop."},
	"recirculation": {"title":"Vortices & Recirculation","explorer":"Swirls can trap energy instead of moving it forward.","engineer":"Separated flow can create recirculation zones, vortices, and added losses."},
	"pressure_loss": {"title":"Pressure Loss","explorer":"Some pushing power is lost as fluid moves.","engineer":"Pressure loss represents dissipative losses through geometry and components."},
	"flow_balance": {"title":"Flow Balance","explorer":"Split the flow evenly when every branch needs its share.","engineer":"Branch balancing controls relative flow distribution across parallel outlets."},
	"bernoulli": {"title":"Bernoulli Principle","explorer":"Speed and pressure trade with each other along a flow path.","engineer":"Bernoulli relates pressure, velocity, and elevation for idealized steady flow."},
	"reynolds": {"title":"Reynolds Number","explorer":"A clue for whether flow stays smooth or gets chaotic.","engineer":"Reynolds number compares inertial and viscous effects and helps characterize flow regime."}
}

func get_card(concept_id: String, mode: String) -> Dictionary:
	if not _concepts.has(concept_id):
		return {}
	var raw: Dictionary = _concepts[concept_id]
	return {"id":concept_id,"title":raw["title"],"body":raw["engineer"] if mode == "engineer" else raw["explorer"]}

func get_unlocked_cards(ids: PackedStringArray, mode: String) -> Array[Dictionary]:
	var cards: Array[Dictionary] = []
	for id in ids:
		var card := get_card(id, mode)
		if not card.is_empty():
			cards.append(card)
	return cards
