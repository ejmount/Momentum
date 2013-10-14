using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Box2DX;
using Box2DX.Common;
using Box2DX.Dynamics;

namespace CrossFire.MVC
{
	class Model
	{
		IList<Body> Q;
		IReadOnlyCollection<Body> Bodies { 
			get {
				return (IReadOnlyCollection<Body>)Q;
			} 
		}


		public void Compute()
		{


		}

	}
}
