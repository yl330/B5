using System;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

namespace TreeSharpPlus
{
    /// <summary>
    ///    Parallel Sequence nodes execute all of their children in parallel. If any
    ///    sequence reports failure, we finish all of the other ticks, but then stop
    ///    all other children and report failure. We report success when all children
    ///    report success.
    /// </summary>
    public class SequenceAll : Parallel
    {
        public SequenceAll(params Node[] children)
            : base(children)
        {
        }

        public override IEnumerable<RunStatus> Execute()
        {
            while (true)
            {
                for (int i = 0; i < this.Children.Count; i++)
                {
                    if (this.childStatus[i] == RunStatus.Running)
                    {
                        Node node = this.Children[i];
                        RunStatus tickResult = this.TickNode(node);

                        // Check to see if anything finished
                        if (tickResult != RunStatus.Running)
                        {
                            // Clean up the node
                            node.Stop();
                            this.childStatus[i] = tickResult;
                            this.runningChildren--;

                        }
                    }
                }

                // If we're out of running nodes, we're done
                if (this.runningChildren == 0)
                {
                    yield return RunStatus.Success;
                    yield break;
                }

                // For forked ticking
                yield return RunStatus.Running;
            }
        }
    }
}