using MiNET.Worlds;

namespace MiNET.Entities
{
	public class Boat : Entity
	{
        public Boat(Player player, Level level): base( 90, level)
		{
            //player.Level.AddEntity(this);
		}
	}
}