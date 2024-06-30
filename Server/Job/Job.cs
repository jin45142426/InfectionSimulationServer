using System;
using System.Collections.Generic;
using System.Text;

namespace Server
{
	//인터페이스를 상속하여 서로 다른 인수를 가지는 작업들을 하나의 타입으로 저장 가능 - 업캐스팅
	public interface IJob
	{
		void Execute();
	}

	//인수가 없는 작업
	public class Job : IJob
	{
		Action _action;

		public Job(Action action)
		{
			_action = action;
		}

		public void Execute()
		{
			_action.Invoke();
		}
	}

	//인수가 1개인 작업
	public class Job<T1> : IJob
	{
		Action<T1> _action;
		T1 _t1;

		public Job(Action<T1> action, T1 t1)
		{
			_action = action;
			_t1 = t1;
		}

		public void Execute()
		{
			_action.Invoke(_t1);
		}
	}

	//인수가 2개인 작업
	public class Job<T1, T2> : IJob
	{
		Action<T1, T2> _action;
		T1 _t1;
		T2 _t2;

		public Job(Action<T1, T2> action, T1 t1, T2 t2)
		{
			_action = action;
			_t1 = t1;
			_t2 = t2;
		}

		public void Execute()
		{
			_action.Invoke(_t1, _t2);
		}
	}

	//인수가 3개인 작업
	public class Job<T1, T2, T3> : IJob
	{
		Action<T1, T2, T3> _action;
		T1 _t1;
		T2 _t2;
		T3 _t3;

		public Job(Action<T1, T2, T3> action, T1 t1, T2 t2, T3 t3)
		{
			_action = action;
			_t1 = t1;
			_t2 = t2;
			_t3 = t3;
		}

		public void Execute()
		{
			_action.Invoke(_t1, _t2, _t3);
		}
	}
}
