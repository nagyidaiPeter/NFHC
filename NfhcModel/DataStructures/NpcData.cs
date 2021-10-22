using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace NfhcModel.DataStructures
{
	public enum NpcTypes
	{
		NeighborActor,
		OlgaActor,
		MotherActor,
		ChilliActor,
		DogActor
	}
	public class NpcData
    {
		public NpcTypes NpcType;

		public Vector3 Position;

		public string Room;

		public string Animation;
	}
}
