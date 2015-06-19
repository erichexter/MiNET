using MiNET.Items;
using MiNET.Worlds;

namespace MiNET.Entities
{
	public class Boat : Entity
	{
	    private readonly Item _item;

	    public Boat( Level level,Item item) : base(90,level)
		{
		    _item = item;
		}
        
	}
}