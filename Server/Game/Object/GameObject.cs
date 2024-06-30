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

		public int ObjectId
		{
			get { return Info.ObjectId; }
			set { Info.ObjectId = value; }
		}

		#endregion

		#region UserInfo UserInfo

		public UserInfo UserInfo { get; set; } = new UserInfo();

		public string Name
		{
			get { return UserInfo.Name; }
			set { UserInfo.Name = value; }
		}

        public string Id
        {
            get { return UserInfo.Id; }
            set { UserInfo.Id = value; }
        }

		public string Position
        {
			get { return UserInfo.Position; }
			set { UserInfo.Position = value; }
        }

        #endregion

        #region MoveInfo MoveInfo

        public MoveInfo MoveInfo { get; set; } = new MoveInfo();

		public CreatureState State
		{
			get { return MoveInfo.State; }
			set { MoveInfo.State = value; }
		}

		public Vector3 Dir
		{
			get
			{
				return new Vector3(MoveInfo.DirX, 0, MoveInfo.DirZ);
			}

			set
			{
				MoveInfo.DirX = value.x;
				MoveInfo.DirZ = value.z;
			}
		}

		public int InputBit
        {
			get { return MoveInfo.InputBit; }
            set
            {
				MoveInfo.InputBit = value;
            }
        }

        #endregion

        #region PosInfo PosInfo

		public PosInfo PosInfo { get; set; } = new PosInfo();

		public Vector3 Pos
        {
            get { return new Vector3(PosInfo.PosX, PosInfo.PosY, PosInfo.PosZ); }
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
			Info.UserInfo = UserInfo;
			Info.MoveInfo = MoveInfo;
			Info.PosInfo = PosInfo;
		}

		public virtual void Update()
		{

		}
	}
}
