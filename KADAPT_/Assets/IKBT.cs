using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;
using RootMotion.FinalIK;

public class IKBT : MonoBehaviour
{
    public Transform wander1;
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;
    public Transform point5;
    public Transform point6;
    public Transform point7;
    public Transform point8;
    public Transform point9;
    public Transform point10;
    public Transform point11;
    public Transform point12;
    public Transform point13;
    public Transform point14;
    public GameObject participant;


    //IK related interface
    public GameObject ball;
    public InteractionObject ikBall;
    public FullBodyBipedEffector hand;
    public FullBodyBipedEffector rightFoot;
    public InteractionObject rightFootAttractor;
    public FullBodyBipedEffector leftFoot;
    public InteractionObject leftFootAttractor;

    private BehaviorAgent behaviorAgent;

    public GameObject door;
    public List<InteractionObject> ikDoor = new List<InteractionObject>();
    InteractionObject doornum;
    bool isOpen = false;
    // Use this for initialization
    void Start()
    {
        behaviorAgent = new BehaviorAgent(this.BuildTreeRoot());
        BehaviorManager.Instance.Register(behaviorAgent);
        behaviorAgent.StartBehavior();
    
    }

    #region IK related function

    protected Node PickUp(GameObject p)
    {
        return new Sequence(this.Node_BallStop(),
                            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(hand, ikBall),
                            new LeafWait(1000),
                            p.GetComponent<BehaviorMecanim>().Node_StopInteraction(hand));
    }

    public Node Node_BallStop()
    {
        return new LeafInvoke(() => {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;

            return RunStatus.Success;
        });
    }

    public Node PutDown(GameObject p)
    {
        return new Sequence(p.GetComponent<BehaviorMecanim>().Node_StartInteraction(hand, ikBall),
                            new LeafWait(300),
                            this.Node_BallMotion(),
                            new LeafWait(500), p.GetComponent<BehaviorMecanim>().Node_StopInteraction(hand));
    }

    public Node Node_BallMotion()
    {
        return new LeafInvoke(() => {
            Rigidbody rb = ball.GetComponent<Rigidbody>();
            rb.velocity = Vector3.zero;
            rb.isKinematic = false;
            ball.transform.parent = null;
            return RunStatus.Success;
        });
    }


    public Node FootTap(GameObject p, FullBodyBipedEffector foot, InteractionObject attractor)
    {
        return new Sequence(
                p.GetComponent<BehaviorMecanim>().Node_StartInteraction(foot, attractor),
                new LeafWait(200),
                p.GetComponent<BehaviorMecanim>().Node_StopInteraction(foot)
            );
    }

    public Node ChaChaRealSmooth(GameObject p, int iterations = 1)
    {
        int counter = 0;

        return new DecoratorInvert(
                new Sequence(
                    new LeafInvoke(() => counter = 0),
                    new DecoratorLoop(
                        new Sequence(
                            new LeafAssert(() => counter < iterations),

                            FootTap(p, leftFoot, leftFootAttractor),
                            FootTap(p, rightFoot, rightFootAttractor),

                            new LeafInvoke(() => counter++)
                        )
                    )
                )
            );
    }

    protected Node OpenDoor(GameObject p)
    {
        //Node open = null;
        //for (int i = 0; i < ikDoor.Count; i++)
        //{
        //    doornum = ikDoor[i];
        //    open = new Sequence(this.Node_OD(),
        //            p.GetComponentz<BehaviorMecanim>().Node_StartInteraction(hand, doornum),
        //            new LeafWait(1000),
        //            p.GetComponent<BehaviorMecanim>().Node_StopInteraction(hand));
        //}
        return new Sequence(this.Node_OD(),
                            p.GetComponent<BehaviorMecanim>().Node_HandAnimation("WOODCUT", true)
                            //new LeafWait(1000),
                            //p.GetComponent<BehaviorMecanim>().Node_HandAnimation("WOODCUT", true)
                            ); 

        //return open;
    }

    public Node Node_OD()
    {
        Node open = null;
        for (int i = 0; i < ikDoor.Count; i++)
        {
            doornum = ikDoor[i];
            open = new LeafInvoke(() => {
                Rigidbody rb = doornum.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;

                return RunStatus.Success;
            });
        }
        //return new LeafInvoke(() => {
        //    Rigidbody rb = door.GetComponent<Rigidbody>();
        //    rb.velocity = Vector3.zero;
        //    rb.isKinematic = false;

        //    return RunStatus.Success;
        //});
        return open;
    }
    //return new LeafInvoke(() => {
    //    Rigidbody rb = doornum.GetComponent<Rigidbody>();
    //    rb.velocity = Vector3.zero;
    //    rb.isKinematic = true;
    //    return RunStatus.Success;
    //});
//}

    public Node Node_DoorMotion(GameObject p)
    {
        Node open = null;
        //for (int i = 0; i < ikDoor.Count; i++)
        //{
            //doornum = p;
            open = new LeafInvoke(() => {
                Rigidbody rb = doornum.GetComponent<Rigidbody>();
                rb.velocity = Vector3.zero;
                rb.isKinematic = true;
                doornum.transform.parent = null;
                p.transform.Rotate(Vector3.down, Space.Self);
                return RunStatus.Success;
            });
        //}
        return open;
    }


    public Node Door(GameObject p)
    {
        Node open = null;
        for (int i = 0; i < ikDoor.Count; i++)
        {
            doornum = ikDoor[i];
            open = new Sequence(p.GetComponent<BehaviorMecanim>().Node_StartInteraction(hand, doornum),
                            new LeafWait(300),
                            this.Node_DoorMotion(p),
                            new LeafWait(500), p.GetComponent<BehaviorMecanim>().Node_StopInteraction(hand));
        }
        return open;
    }
    //{
    //    return new Sequence(p.GetComponent<BehaviorMecanim>().Node_StartInteraction(hand, doornum),
    //        new LeafWait(300),
    //        this.Node_DoorMotion(),
    //        new LeafWait(500), p.GetComponent<BehaviorMecanim>().Node_StopInteraction(hand));
    //}

    #endregion

    protected Node ST_ApproachAndWait(Transform target)
    {
        Val<Vector3> position = Val.V(() => target.position);
        return new Sequence(participant.GetComponent<BehaviorMecanim>().Node_GoTo(position), new LeafWait(1000));
    }

    //protected Node BuildTreeRoot()
    //{
    //    Node roaming = new DecoratorLoop(
    //            new Selector(
    //                new Sequence(
    //                    this.ST_ApproachAndWait(this.wander1),
    //                    new DecoratorLoop(
    //                        //new Sequence(this.PickUp(participant), ChaChaRealSmooth(participant, 3), this.PutDown(participant)))
    //                        new Sequence(
    //                            participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => ball.transform.position)),
    //                            this.PickUp(participant), 
    //                            this.PutDown(participant)
    //                        )
    //                    )
    //                ),
    //                this.ST_ApproachAndWait(this.wander2)
    //            )
    //        );
    //    return roaming;
    //}

    public Node doorOpen(Transform point)
    {
        return new LeafInvoke(() =>
        {
            Collider[] door = Physics.OverlapSphere(point.position, 5f);
            for (int i = 0; i < door.Length; ++i) {

                if (door[i].tag.Equals("door"))
                {
                    print("detected" + door[0]);
                    return RunStatus.Failure; }
            }
            print("not detected" + door[0]);
            return RunStatus.Success;
        });
    }

    public Node openTheDoor(Transform point)
    {
        return new LeafInvoke(() =>
        {
            Rigidbody door;
            Collider[] doors = Physics.OverlapSphere(point.position, 5f);
            for (int i = 0; i < doors.Length; ++i)
            {

                if (doors[i].tag.Equals("door"))
                {
                    print("in");
                    door = doors[i].GetComponent<Rigidbody>();
                    OpenDoor(participant);
                    Vector3 pos = door.transform.position;
                    pos.z = pos.z + 3f;
                    door.transform.position = pos;
                }
            }
            return RunStatus.Success;
        });
    }
    protected Node BuildTreeRoot()
    {
        Node story =
                    new SequenceAll(
                        //this.ST_ApproachAndWait(this.wander1),
                        new SequenceShuffle(
                            //new Sequence(this.PickUp(participant), ChaChaRealSmooth(participant, 3), this.PutDown(participant)))
                            new Sequence(
                                this.ST_ApproachAndWait(this.point1),this.OpenDoor(participant), new LeafWait(1000),
                                new Selector(
                                    doorOpen(this.point1), openTheDoor(this.point1)
                                    ),
                                this.ST_ApproachAndWait(this.point10)
                                )

                            )
        //copy 13 times


        );
        return story;
    }

    //protected Node BuildTreeRoot()
    //{
    //    Node roaming = 
    //            new SequenceAll(
    //                    this.ST_ApproachAndWait(this.point7),
    //                    new SequenceShuffle(
    //                        //new Sequence(this.PickUp(participant), ChaChaRealSmooth(participant, 3), this.PutDown(participant)))
    //                        new Sequence(
    //                            participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => door.transform.position)),
    //                            this.Door(participant),
    //                            this.OpenDoor(participant,door),
    //                            this.ST_ApproachAndWait(this.point14),
    //                            participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => ball.transform.position)),
    //                            this.PickUp(participant),
    //                            this.PutDown(participant)
    //                    )
    //                ),
    //         this.ST_ApproachAndWait(this.point7)
            
    //                )
    //        ;
    //    return roaming;
    //}
}
