using UnityEngine;
using System.Collections.Generic;

namespace BanpoFri
{
    [System.Serializable]
    public class WaveInfoData
    {
        [SerializeField]
		private int _order;
		public int order
		{
			get { return _order;}
			set { _order = value;}
		}
		[SerializeField]
		private int _wave;
		public int wave
		{
			get { return _wave;}
			set { _wave = value;}
		}
		[SerializeField]
		private int _stage;
		public int stage
		{
			get { return _stage;}
			set { _stage = value;}
		}
		[SerializeField]
		private List<int> _unit_idx;
		public List<int> unit_idx
		{
			get { return _unit_idx;}
			set { _unit_idx = value;}
		}
		[SerializeField]
		private List<int> _unit_hp;
		public List<int> unit_hp
		{
			get { return _unit_hp;}
			set { _unit_hp = value;}
		}
		[SerializeField]
		private int _unit_appear_time;
		public int unit_appear_time
		{
			get { return _unit_appear_time;}
			set { _unit_appear_time = value;}
		}

    }

    [System.Serializable]
    public class WaveInfo : Table<WaveInfoData, KeyValuePair<int,int>>
    {
    }
}

