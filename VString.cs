namespace Jump_Bruteforcer
{
    public class VPlayer
    {
        double Y, VSpeed;
        bool GoalHeightReached;
        int Frame;
        Input CurrentInputs; // the inputs for the current frame

        public List<double> VString { get; } // does not include initial position
        public readonly SortedDictionary<int, Input> InputHistory;

        public int LowestGoal { get; set; }

        private VPlayer(double Y, double VSpeed, int LowestGoal)
        {
            this.Y = Y;
            this.VSpeed = VSpeed;
            Frame = 0;
            GoalHeightReached = Y <= LowestGoal;

            VString = new List<double>() {Y};
            InputHistory = new();

            this.LowestGoal = LowestGoal;
        }

        private VPlayer(VPlayer vp)
        {
            Y = vp.Y;
            VSpeed = vp.VSpeed;
            Frame = vp.Frame;
            GoalHeightReached = vp.GoalHeightReached;
            LowestGoal = vp.LowestGoal;

            CurrentInputs = vp.CurrentInputs;

            VString = new(vp.VString);
            InputHistory = new(vp.InputHistory);
        }

        private void Jump(bool SingleJump)
        {
            VSpeed = SingleJump ? PhysicsParams.SJUMP_VSPEED : PhysicsParams.DJUMP_VSPEED;
            CurrentInputs |= Input.Jump;
        }

        private bool CanRelease()
            => VSpeed < 0;

        private bool Advance(bool Release)
        {
            // assuming releases only used when CanRelease true
            if (Release)
            {
                VSpeed *= PhysicsParams.RELEASE_MULTIPLIER;
            }
            else if (VSpeed > PhysicsParams.MAX_VSPEED)
            {
                VSpeed = PhysicsParams.MAX_VSPEED;
            }

            VSpeed += PhysicsParams.GRAVITY;

            Y += VSpeed;

            bool AboveGoal = Math.Round(Y) <= LowestGoal;
            if (GoalHeightReached && !AboveGoal)
            {
                // first frame under the goal height,
                // returning early since this frame is not useful in the vstring
                return false;
            }

            VString.Add(Y);
            if (Release)
            {
                CurrentInputs |= Input.Release;
            }

            if (CurrentInputs != 0)
            {
                InputHistory.Add(Frame, CurrentInputs);
                CurrentInputs = 0;
            }

            GoalHeightReached = GoalHeightReached || AboveGoal;
            Frame++;
            return AboveGoal;
        }

        /// <summary>
        /// Returns a list of VStrings which reach the specified height.
        /// </summary>
        /// <param name="Y"> The initial Y Position of the VStrings. </param>
        /// <param name="SingleJump"> Determines whether the VStrings start with a singlejump or doublejump. </param>
        /// <param name="LowestGoal"> A lowest pixel height the VStrings have to reach. </param>
        /// <returns></returns>
        public static List<VPlayer> GenerateVStrings(double Y, bool SingleJump, int LowestGoal)
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
                        VPlayer NewPlayer = new(Player);
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
