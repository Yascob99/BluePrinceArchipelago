using BluePrinceArchipelago.Rooms;
using System.Collections.Generic;

namespace BluePrinceArchipelago.Archipelago.Commands
{
    /// <summary>
    ///     A command for forcing a room to appear in drafting when next logically possible.
    /// </summary>
    /// <param name="name">The name of the command.</param>
    public class ForceCommand(string name) : Command(name)
    {
        private readonly string _Description = "Forces a draft of the room when next possible";
        public override string Description
        {
            get { return _Description; }
        }
        private readonly string _Syntax = "Usage\n\t/Force <Room>\n\t/Force <Room>";
        public override string Syntax
        {
            get { return _Syntax; }
        }
        public override void Run(List<string> Args)
        {
            string roomName = string.Join(" ", Args);
            ModRoom room = ModRoomManager.GetRoomByName(roomName);
            if (room != null)
            {
                ModRoomManager.ForceRoomQueue.Add(room);
            }
        }
    }
}
