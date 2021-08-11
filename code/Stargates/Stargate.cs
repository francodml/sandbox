using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;
using Sandbox.UI;

namespace winsandbox.Stargates
{
	[Library("ent_stargate", Spawnable = true, Title = "Stargate")]
	[Hammer.EditorModel("models/stargates/stargate.vmdl")]
	public partial class Stargate : AnimEntity, IUse
	{
		[Net]
		[Property("Address", Group = "Stargate")]
		public string Address { get; private set; }
		[Net]
		public bool Busy { get; private set; }

		private string OtherAddress;
		private Stargate OtherGate;

		private EventHorizon eventHorizon;
		public override void Spawn()
		{
			base.Spawn();

			Tags.Add( "IsStargate" );
			SetModel( "models/stargates/stargate.vmdl" );

			SetupPhysicsFromModel(PhysicsMotionType.Dynamic,false);
			PhysicsBody.BodyType = PhysicsBodyType.Static;

			if ( IsServer && string.IsNullOrEmpty( Address ) )
			{
				List<char> PotentialSymbols = Utils.AddressSymbols;
				for (int i = 0; i < 7; i++ )
				{
					int random = Rand.Int( 0, PotentialSymbols.Count-1 );
					Address += PotentialSymbols[random];
					PotentialSymbols.RemoveAt( random );
				}
			}
			eventHorizon = new();
			eventHorizon.Position = Position;
			eventHorizon.Rotation = Rotation;
			eventHorizon.SetParent( this );

		}
		public bool IsUsable( Entity user ) => true;
		public bool OnUse( Entity user )
		{
			ChatBox.AddInformation(To.Single(user),Address);
			/*var otherGate = (Stargate)Entity.All.Where( x => x is Stargate a && a.Address != Address ).FirstOrDefault();
			otherGate.Glow();*/
			switch ( eventHorizon.Enabled )
			{
				case true:
					eventHorizon.Disable();
					break;
				case false:
					eventHorizon.Enable();
					break;
			}

			return false;
		}

		public async void Glow()
		{
			GlowActive = true;
			await Task.DelaySeconds( 5 );
			GlowActive = false;
		}

		public void FindGate(string address = null)
		{
			if ( address == null && OtherAddress != null )
				address = OtherAddress;
			else return;

			OtherGate = (Stargate)Entity.All.Where( x => x is Stargate a && a.Address == address ).FirstOrDefault();
		}

		public bool Connect(string address = null)
		{
			if ( address == null && OtherAddress == null )
				return false;
			if ( Busy )
				return false;
			return true;
		}
	}
}
