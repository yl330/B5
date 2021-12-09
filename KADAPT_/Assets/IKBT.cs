using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TreeSharpPlus;
using RootMotion.FinalIK;
using System;
using UnityEngine.UI;

public class IKBT : MonoBehaviour
{
    public Transform point1;
    public Transform point2;
    public Transform point3;
    public Transform point4;
    //public Transform point5;
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
    public GameObject participant2;
    public GameObject participant3;
    public GameObject participant4;
    public Hashtable count;
    private Vector3 v1 = new Vector3();

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

    public Text countTextP1;
    public Text countTextP2;
    public Text countTextP3;
    public Text winText;
    public GameObject winner;
    public GameObject winner2;
    private int count1, count2, count3 = 0;


    // Use this for initialization
    void Start()
    {

        winText.text = " ";
        count = new Hashtable();
        count.Add(participant, 0);
        count.Add(participant2, 0);
        count.Add(participant3, 0);
        behaviorAgent = new BehaviorAgent(this.BuildTreeRoot());
        BehaviorManager.Instance.Register(behaviorAgent);
        behaviorAgent.StartBehavior();
        SetCountText(count[participant], count[participant2], count[participant3]);

    }

    #region IK related function

    protected Node PickUp(GameObject p, InteractionObject ikobj)
    {
        return new Sequence(this.Node_BallStop(),
                            p.GetComponent<BehaviorMecanim>().Node_StartInteraction(hand, ikobj),
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

        return new Sequence(this.Node_OD(),
                            p.GetComponent<BehaviorMecanim>().Node_HandAnimation("WOODCUT", true),
                            new LeafWait(1000),
                            p.GetComponent<BehaviorMecanim>().Node_HandAnimation("WOODCUT", false),
                            new LeafInvoke(() => { return RunStatus.Failure; })
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

    #endregion

    protected Node ST_ApproachAndWait(Transform target, GameObject participant)
    {
        Val<Vector3> position = Val.V(() => target.position);
        print(target.position.x + " " + target.position.y + target.position.z);
        return new Sequence(participant.GetComponent<BehaviorMecanim>().Node_GoTo(position), new LeafWait(1000));
    }
    protected Node ST(GameObject participant)
    {
        Val<Vector3> position = Val.V(() => v1);
        return new Sequence(participant.GetComponent<BehaviorMecanim>().Node_GoTo(position), new LeafWait(1000));
    }
    protected Node ST_Approach(Vector3 target, GameObject participant)
    {
        print("iiiiiiiiiiiiiiiiiiiiii");
        Val<Vector3> position = Val.V(() => target);

        print(target.x + " " + target.y + " " + target.z);
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
            Collider[] door = Physics.OverlapSphere(point.position, 3f);
            for (int i = 0; i < door.Length; ++i)
            {

                if (door[i].tag.Equals("door"))
                {
                    //print("detected" + door[0]);
                    return RunStatus.Failure;
                }
            }
            //print("not detected" + door[0]);
            return RunStatus.Success;
        });
    }

    public Node openTheDoor(Transform point, GameObject participant)
    {
        return new LeafInvoke(() =>
        {
            Rigidbody door;
            Collider[] doors = Physics.OverlapSphere(point.position, 3f);
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

    public Node havePrice(Transform point)
    {
        return new LeafInvoke(() =>
        {
            Collider[] price = Physics.OverlapSphere(point.position, 4f);
            for (int i = 0; i < price.Length; ++i)
            {

                if (price[i].tag.Equals("price"))
                {
                    print("detected");
                    return RunStatus.Failure;
                }
            }
            print("not detected");
            return RunStatus.Success;
        });
    }
    public Node disappear(GameObject participant)
    {
        return new LeafInvoke(() =>
        {
            Rigidbody price;
            Collider[] prices = Physics.OverlapSphere(participant.transform.position, 4f);
            for (int i = 0; i < prices.Length; ++i)
            {

                if (prices[i].tag.Equals("price"))
                {
                    price = prices[i].GetComponent<Rigidbody>();
                    prices[i].enabled = false;

                }

            }
            return RunStatus.Success;
        });
    }
    public Node PickUpPrice(Transform point, GameObject participant)
    {
        return new LeafInvoke(() =>
        {
            Rigidbody price;
            Collider[] prices = Physics.OverlapSphere(point.position, 4f);
            for (int i = 0; i < prices.Length; ++i)
            {

                if (prices[i].tag.Equals("price"))
                {
                    print("pick");
                    price = prices[i].GetComponent<Rigidbody>();
                    v1 = price.transform.position;
                    print(v1);
                    ST_Approach(price.transform.position, participant).Start();
                    PickUp(participant, prices[i].GetComponent<InteractionObject>()).Start();
                    //prices[i].enabled = false;
                    //curp1 = prices[i];
                    count[participant] = (int)count[participant] + 1;
                    SetCountText(count[participant], count[participant2], count[participant3]);
                    print(count[participant]);
                }

            }
            return RunStatus.Failure;
        });
    }

    public void SetCountText(object c1, object c2, object c3)
    {
        countTextP1.text = "Daniel1 Score: " + c1.ToString();
        countTextP2.text = "Daniel2 Score: " + c2.ToString();
        countTextP3.text = "Daniel3 Score: " + c3.ToString();

        count1 = (int)c1;
        count2 = (int)c2;
        count3 = (int)c3;

    }

    
    public Node userInteract()
    {
        return new LeafInvoke(() =>
        {
            if (Input.GetKey("1"))
            {
                return RunStatus.Failure;
            }
            return RunStatus.Success;
        });
    }
    public Node userInteract2()
    {
        return new LeafInvoke(() =>
        {
            if (Input.GetKey("2"))
            {
                return RunStatus.Failure;
            }
            return RunStatus.Success;
        });
    }
    public Node userInteract3()
    {
        return new LeafInvoke(() =>
        {
            if (Input.GetKey("3"))
            {
                return RunStatus.Failure;
            }
            return RunStatus.Success;
        });
    }

    private Node Cheer()
    {
        return new Sequence(

            winner.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", true),
            new LeafWait(10000),
            winner.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", false),
            new LeafInvoke(() => { return RunStatus.Success; })
         );
    }

    private Node Cheer2()
    {
        
        return new Sequence(
            winner.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", true),
            winner2.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", true),
            new LeafWait(10000),
            winner.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", false),
            winner2.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", false),
            new LeafInvoke(() => { return RunStatus.Success; })
         );
    }

    public Node Faster(GameObject g)
    {
        return new LeafInvoke(() =>
        {
            g.GetComponent<UnitySteeringController>().maxSpeed = 8f;
            return RunStatus.Failure;
        });
    }
    protected Node BuildTreeRoot()
    {
        Node story = new Sequence(
                    new SequenceAll(
                        //ST_Approach(new Vector3(21.7f, 0.15f, 22.612f), participant),
                        new SequenceAll(
                        new Sequence(
                        new SequenceShuffle(

                            new Sequence(
                                this.ST_ApproachAndWait(this.point1, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.forward)),
                                new Selector(doorOpen(this.point1), this.OpenDoor(participant), openTheDoor(this.point1, participant)),
                                    ST_Approach(new Vector3(40f, 0f, 33f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                                    ),

                            new Sequence(
                                this.ST_ApproachAndWait(this.point2, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.right)),
                                new Selector(
                                    doorOpen(this.point2), this.OpenDoor(participant), openTheDoor(this.point2, participant)),
                                    ST_Approach(new Vector3(20f, 0f, 10f), participant),
                                    new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                                ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point3, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.right)),
                                new Selector(doorOpen(this.point3), this.OpenDoor(participant), openTheDoor(this.point3, participant)),
                                ST_Approach(new Vector3(20f, 0f, 25f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point4, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.right)),
                                new Selector(doorOpen(this.point4), this.OpenDoor(participant), openTheDoor(this.point4, participant)),
                                ST_Approach(new Vector3(22f, 0f, 39f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            ),
                            //new Sequence(
                            //    this.ST_ApproachAndWait(this.point5, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.left)),
                            //    new Selector(doorOpen(this.point5), this.OpenDoor(participant), openTheDoor(this.point5, participant)),
                            //    ST_Approach(new Vector3(0f, 0f, 14.3f), participant),
                            //    new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            //),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point6, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point6), this.OpenDoor(participant), openTheDoor(this.point6, participant)),
                                ST_Approach(new Vector3(1f, 0f, 23f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point7, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point7), this.OpenDoor(participant), openTheDoor(this.point7, participant)),
                                ST_Approach(new Vector3(0.21f, 0f, 32f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point8, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point8), this.OpenDoor(participant), openTheDoor(this.point8, participant)),
                                ST_Approach(new Vector3(2f, 0f, 40f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point9, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.forward)),
                                new Selector(doorOpen(this.point9), this.OpenDoor(participant), openTheDoor(this.point9, participant)),
                                ST_Approach(new Vector3(11f, 0f, 46f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point10, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point10), this.OpenDoor(participant), openTheDoor(this.point10, participant)),
                                ST_Approach(new Vector3(-23f, 0f, 12f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point11, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point11), this.OpenDoor(participant), openTheDoor(this.point11, participant)),
                                ST_Approach(new Vector3(-23f, 0f, 22f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point12, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point12), this.OpenDoor(participant), openTheDoor(this.point12, participant)),
                                ST_Approach(new Vector3(-23f, 0f, 41f), participant),
                                new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            )
                            ), ST_Approach(new Vector3(39f, 0f, -12f), participant)
                            ), new DecoratorLoop(new Selector(this.userInteract(), Faster(participant)))
                        ),
new SequenceAll(
                        new Sequence(
                        new SequenceShuffle(

                            new Sequence(
                                this.ST_ApproachAndWait(this.point1, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.forward)),
                                new Selector(doorOpen(this.point1), this.OpenDoor(participant2), openTheDoor(this.point1, participant2)),
                                    ST_Approach(new Vector3(40f, 0f, 33f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                                    ),

                            new Sequence(
                                this.ST_ApproachAndWait(this.point2, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.right)),
                                new Selector(
                                    doorOpen(this.point2), this.OpenDoor(participant2), openTheDoor(this.point2, participant2)),
                                    ST_Approach(new Vector3(20f, 0f, 10f), participant2),
                                    new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                                ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point3, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.right)),
                                new Selector(doorOpen(this.point3), this.OpenDoor(participant2), openTheDoor(this.point3, participant2)),
                                ST_Approach(new Vector3(20f, 0f, 25f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point4, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.right)),
                                new Selector(doorOpen(this.point4), this.OpenDoor(participant2), openTheDoor(this.point4, participant2)),
                                ST_Approach(new Vector3(22f, 0f, 39f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                            ),
                            //new Sequence(
                            //    this.ST_ApproachAndWait(this.point5, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.left)),
                            //    new Selector(doorOpen(this.point5), this.OpenDoor(participant), openTheDoor(this.point5, participant)),
                            //    ST_Approach(new Vector3(0f, 0f, 14.3f), participant),
                            //    new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant), new Sequence(ST(participant), disappear(participant)))
                            //),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point6, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point6), this.OpenDoor(participant2), openTheDoor(this.point6, participant2)),
                                ST_Approach(new Vector3(1f, 0f, 23f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point7, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point7), this.OpenDoor(participant2), openTheDoor(this.point7, participant2)),
                                ST_Approach(new Vector3(0.21f, 0f, 32f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point8, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point8), this.OpenDoor(participant2), openTheDoor(this.point8, participant2)),
                                ST_Approach(new Vector3(2f, 0f, 40f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point9, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.forward)),
                                new Selector(doorOpen(this.point9), this.OpenDoor(participant2), openTheDoor(this.point9, participant2)),
                                ST_Approach(new Vector3(11f, 0f, 46f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point10, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point10), this.OpenDoor(participant2), openTheDoor(this.point10, participant2)),
                                ST_Approach(new Vector3(-23f, 0f, 12f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point11, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point11), this.OpenDoor(participant2), openTheDoor(this.point11, participant2)),
                                ST_Approach(new Vector3(-23f, 0f, 22f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point12, participant2), participant2.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant2.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point12), this.OpenDoor(participant2), openTheDoor(this.point12, participant2)),
                                ST_Approach(new Vector3(-23f, 0f, 41f), participant2),
                                new Selector(havePrice(participant2.transform), PickUpPrice(participant2.transform, participant2), new Sequence(ST(participant2), disappear(participant2)))
                            )
                            ), ST_Approach(new Vector3(39f, 0f, -12f), participant2)
                            ), new DecoratorLoop(new Selector(this.userInteract2(), Faster(participant2)))
                        ),
new SequenceAll(
                        new Sequence(
                        new SequenceShuffle(

                            new Sequence(
                                this.ST_ApproachAndWait(this.point1, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.forward)),
                                new Selector(doorOpen(this.point1), this.OpenDoor(participant3), openTheDoor(this.point1, participant3)),
                                    ST_Approach(new Vector3(40f, 0f, 33f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                                    ),

                            new Sequence(
                                this.ST_ApproachAndWait(this.point2, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.right)),
                                new Selector(
                                    doorOpen(this.point2), this.OpenDoor(participant3), openTheDoor(this.point2, participant3)),
                                    ST_Approach(new Vector3(20f, 0f, 10f), participant3),
                                    new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                                ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point3, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.right)),
                                new Selector(doorOpen(this.point3), this.OpenDoor(participant3), openTheDoor(this.point3, participant3)),
                                ST_Approach(new Vector3(20f, 0f, 25f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point4, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.right)),
                                new Selector(doorOpen(this.point4), this.OpenDoor(participant3), openTheDoor(this.point4, participant3)),
                                ST_Approach(new Vector3(22f, 0f, 39f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            ),
                            //new Sequence(
                            //    this.ST_ApproachAndWait(this.point5, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.left)),
                            //    new Selector(doorOpen(this.point5), this.OpenDoor(participant3), openTheDoor(this.point5, participant3)),
                            //    ST_Approach(new Vector3(0f, 0f, 14.3f), participant3),
                            //    new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            //),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point6, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point6), this.OpenDoor(participant3), openTheDoor(this.point6, participant3)),
                                ST_Approach(new Vector3(1f, 0f, 23f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point7, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point7), this.OpenDoor(participant3), openTheDoor(this.point7, participant3)),
                                ST_Approach(new Vector3(0.21f, 0f, 32f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point8, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point8), this.OpenDoor(participant3), openTheDoor(this.point8, participant3)),
                                ST_Approach(new Vector3(2f, 0f, 40f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point9, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.forward)),
                                new Selector(doorOpen(this.point9), this.OpenDoor(participant3), openTheDoor(this.point9, participant3)),
                                ST_Approach(new Vector3(11f, 0f, 46f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point10, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point10), this.OpenDoor(participant3), openTheDoor(this.point10, participant3)),
                                ST_Approach(new Vector3(-23f, 0f, 12f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point11, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point11), this.OpenDoor(participant3), openTheDoor(this.point11, participant3)),
                                ST_Approach(new Vector3(-23f, 0f, 22f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            ),
                            new Sequence(
                                this.ST_ApproachAndWait(this.point12, participant3), participant3.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant3.transform.position + Vector3.left)),
                                new Selector(doorOpen(this.point12), this.OpenDoor(participant3), openTheDoor(this.point12, participant3)),
                                ST_Approach(new Vector3(-23f, 0f, 41f), participant3),
                                new Selector(havePrice(participant3.transform), PickUpPrice(participant3.transform, participant3), new Sequence(ST(participant3), disappear(participant3)))
                            )
                            ), ST_Approach(new Vector3(39f, 0f, -12f), participant3)
                            ), new DecoratorLoop(new Selector(this.userInteract3(), Faster(participant3)))
                        )
),  new Selector(Check(), Cheer2()), new Selector(Check2(), Cheer()), new DecoratorLoop(new LeafWait(1000)), new LeafInvoke(() => { return RunStatus.Running; }));
        return story;
    }
    public Node Check2()
    {
        return new LeafInvoke(() =>
        {
            int score1 = count1;
            int score2 = count2;
            int score3 = count3;

            if (score1 > score2 && score1 > score3)
            {
                winText.text = "Daniel1 won!";
                winner = participant;
                return RunStatus.Failure;
            }
            if (score2 > score1 && score2 > score3)
            {
                winText.text = "Daniel2 won!";
                winner = participant2;
                return RunStatus.Failure;
            }
            if (score3 > score1 && score3 > score2)
            {
                winText.text = "Daniel3 won!";
                winner = participant3;
                return RunStatus.Failure;
            }
            //if (score1 == score2)
            //{
            //    //winText.text = "Daniel1 and Daniel2 won!";
            //    //winner = participant;
            //    //winner2 = participant2;

            //}
            //if (score1 == score3)
            //{
            //    //winText.text = "Daniel1 and Daniel3 won!";
            //    //winner = participant;
            //    //winner2 = participant2;

            //}
            //if (score2 == score3)
            //{
            //    //winText.text = "Daniel2 and Daniel3 won!";
            //    //winner = participant;
            //    //winner2 = participant2;

            //}

            return RunStatus.Success;
        });
    }
    public Node Check()
    {
        return new LeafInvoke(() =>
        {
            int score1 = count1;
            int score2 = count2;
            int score3 = count3;

            //if (score1 > score2 && score1 > score3)
            //{
            //    //winText.text = "Daniel1 won!";
            //    winner = participant;
            //}
            //if (score2 > score1 && score2 > score3)
            //{
            //    //winText.text = "Daniel2 won!";
            //    winner = participant2;
            //}
            //if (score3 > score1 && score3 > score2)
            //{
            //    //winText.text = "Daniel3 won!";
            //    winner = participant3;
            //}
            if (score1 == score2)
            {
                winText.text = "Daniel1 and Daniel2 won!";
                winner = participant;
                winner2 = participant2;
                return RunStatus.Failure;
            }
            if (score1 == score3)
            {
                winText.text = "Daniel1 and Daniel3 won!";
                winner = participant;
                winner2 = participant2;
                return RunStatus.Failure;
            }
            if (score2 == score3)
            {
                winText.text = "Daniel2 and Daniel3 won!";
                winner = participant;
                winner2 = participant2;
                return RunStatus.Failure;
            }

            return RunStatus.Success;
        });
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
