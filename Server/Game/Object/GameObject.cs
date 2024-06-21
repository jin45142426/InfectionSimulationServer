using Google.Protobuf.Protocol;
using System;
using System.Collections.Generic;
using System.Text;

namespace Server.Game
{
	public class GameObject
	{
		public GameObjectType ObjectType { get; protected set; } = GameObjectType.None;

		public GameRoom Room { get; set; }

		#region ObjectInfo Info

		public ObjectInfo Info { get; set; } = new ObjectInfo();

		public int Id
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}

		public string Name
		{
			get { return Info.Name; }
			set { Info.Name = value; }
		}

		#endregion

		#region PositionInfo PosInfo

		public PositionInfo PosInfo { get; private set; } = new PositionInfo();

		public CreatureState State
		{
			get { return PosInfo.State; }
			set { PosInfo.State = value; }
		}

		public Vector3 Pos
		{
			get
			{
				return new Vector3(PosInfo.PosX, PosInfo.PosY, PosInfo.PosZ);
			}

			set
			{
				PosInfo.PosX = value.x;
				PosInfo.PosY = value.y;
				PosInfo.PosZ = value.z;
			}
		}

		#endregion

		public GameObject()
		{
			Info.PosInfo = PosInfo;
		}

		public virtual void Update()
		{

		}
	}
}
