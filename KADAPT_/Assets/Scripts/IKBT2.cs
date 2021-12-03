using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;
using RootMotion.FinalIK;
using RootMotion.FinalIK.Demos;

public class IKBT2 : MonoBehaviour
{
    public GameObject participant;
    public GameObject participant2;

    //IK related interface
    public InteractionObject shakePoint;
    public InteractionObject shakePoint2;
    public FullBodyBipedEffector hand;
    public FullBodyBipedEffector hand2;

    private BehaviorAgent behaviorAgent;
    // Use this for initialization
    void Start()
    {
        behaviorAgent = new BehaviorAgent(this.BuildTreeRoot());
        BehaviorManager.Instance.Register(behaviorAgent);
        behaviorAgent.StartBehavior();
    }

    #region IK related function

    private Node ShakeHands()
    {
        return new Sequence(
                new Sequence(
                    new LeafInvoke(() => {
                        var dir = Vector3.back;
                        dir.Normalize();
                        var pos = participant.transform.position;
                        
                        pos.y = 5;
                        shakePoint.transform.position = pos + dir;
                        shakePoint2.transform.position = pos - dir;

                        //shakePoint.transform.rotation = Quaternion.LookRotation(Vector3.zero, Vector3.left);
                        //shakePoint2.transform.rotation = Quaternion.LookRotation(Vector3.zero, Vector3.left);
                    }),
                    new SequenceParallel(
                        //participant.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER",true)
                        participant.GetComponent<BehaviorMecanim>().Node_StartInteraction(hand, shakePoint),
                        participant.GetComponent<BehaviorMecanim>().Node_StartInteraction(hand2, shakePoint2)
                    ),
                    new LeafWait(1000),
                    new SequenceParallel(
                        //participant.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", false)
                       participant.GetComponent<BehaviorMecanim>().Node_StopInteraction(hand),
                       participant.GetComponent<BehaviorMecanim>().Node_StopInteraction(hand2)
                    )
                )
            );
    }

    //protected Node PickUp(GameObject p)
    //{
    //    return new Sequence(this.Node_BallStop(),
    //                        p.GetComponent<BehaviorMecanim>().Node_StartInteraction(hand, ikBall),
    //                        new LeafWait(1000),
    //                        p.GetComponent<BehaviorMecanim>().Node_StopInteraction(hand));
    //}
    
    //public Node PutDown(GameObject p)
    //{
    //    return new Sequence(p.GetComponent<BehaviorMecanim>().Node_StartInteraction(hand, ikBall),
    //                        new LeafWait(300),
    //                        this.Node_BallMotion(),
    //                        new LeafWait(500), p.GetComponent<BehaviorMecanim>().Node_StopInteraction(hand));
    //}
    
    #endregion

    protected Node ST_Approach()
    {
        Val<Vector3> position = Val.V(() => participant2.transform.position);
        Val<Vector3> position2 = Val.V(() => participant.transform.position);
        return new Sequence(
                participant.GetComponent<BehaviorMecanim>().Node_GoToUpToRadius(position, 2),
                participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(position),
                participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(position2)
            );
    }

    protected Node BuildTreeRoot()
    {
        return new Sequence(
                this.ST_Approach(),
                new DecoratorLoop(
                    new Sequence(
                        ShakeHands()
                    )
                )
            );
    }
}
