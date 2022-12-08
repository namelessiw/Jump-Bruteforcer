using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jump_Bruteforcer
{
    public class VPlayer
    {
        const double GRAVITY = 0.4, MAX_VSPEED = 9, RELEASE_MULTIPLIER = 0.45, SJUMP_VSPEED = -8.5, DJUMP_VSPEED = -7;
        double Y, VSpeed;
        bool GoalHeightReached;

        readonly List<double> VString;
        readonly List<bool> Releases;

        // could also use int and use rounded position to check if on same pixel
        static double LowestGoal;

        public VPlayer(double Y, double VSpeed, double LowestGoal)
        {
            this.Y = Y;
            this.VSpeed = VSpeed;
            GoalHeightReached = Y <= LowestGoal;

            VString = new List<double>();
            Releases = new List<bool>();

            VPlayer.LowestGoal = LowestGoal;
        }

        public VPlayer(VPlayer vp)
        {
            Y = vp.Y;
            VSpeed = vp.VSpeed;
            GoalHeightReached = vp.GoalHeightReached;

            VString = new List<double>(vp.VString);
            Releases = new List<bool>(vp.Releases);
        }

        private void Jump(bool SingleJump)
        {
            VSpeed = SingleJump ? SJUMP_VSPEED : DJUMP_VSPEED;
        }

        private bool CanRelease()
            => VSpeed < 0;

        private bool Advance(bool Release)
        {
            // assuming releases only used when CanRelease true
            if (Release)
            {
                VSpeed *= RELEASE_MULTIPLIER;
            }
            else if (VSpeed > MAX_VSPEED)
            {
                VSpeed = MAX_VSPEED;
            }

            VSpeed += GRAVITY;

            Y += VSpeed;

            bool AboveGoal = Y <= LowestGoal;
            if (GoalHeightReached && !AboveGoal)
            {
                // first frame under the goal height,
                // returning early since this frame is not useful in the vstring
                return false;
            }

            VString.Add(Y);
            Releases.Add(Release);

            GoalHeightReached = GoalHeightReached || AboveGoal;
            return AboveGoal;
        }

        public static List<VPlayer> GenerateVStrings(double Y, bool SingleJump, double LowestGoal)
        {
            VPlayer Root = new(Y, 0, LowestGoal);
            Root.Jump(SingleJump);

            List<VPlayer> Result = new();
            Queue<VPlayer> Players = new();
            Players.Enqueue(Root);

            while (Players.Count > 0)
            {
                VPlayer Player = Players.Peek();

                if (Player.CanRelease())
                {
                    do
                    {
                        VPlayer NewPlayer = new VPlayer(Player);
                        NewPlayer.Advance(false);
                        Players.Enqueue(NewPlayer);

                        Player.Advance(true);
                    }
                    while (Player.CanRelease());
                }
                else
                {
                    Players.Dequeue();
                    if (Player.GoalHeightReached)
                    {
                        while (Player.Advance(false)) ;
                        Result.Add(Player);
                    }
                }
            }

            return Result;
        }
    }
}
