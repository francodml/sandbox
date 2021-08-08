using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox;

namespace winsandbox.entities
{
	[Library("ent_sex", Title = "Good Entity", Spawnable = true)]
    class GoodEntity : Prop, IUse
	{
		public override void Spawn()
		{
			base.Spawn();

			SetModel("models/props/cs_office/chair_office.vmdl_c");
		}

		public bool IsUsable(Entity User) => true;

		public bool OnUse(Entity User)
		{
			if ( !IsServer ) return false;
			Log.Info( this.NetworkIdent );
			Log.Info(Entity.FindByIndex(this.NetworkIdent));
			return false;
		}
	}
}
