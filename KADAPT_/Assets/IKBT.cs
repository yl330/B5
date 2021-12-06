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
    public GameObject participant2;
    public GameObject participant3;
    public GameObject participant4;
    public Hashtable count;
  //  private Vector3 11;

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

    public Text countTextP1;
    public Text countTextP2;
    public Text countTextP3;
    public Text winText;
    public GameObject winner;

    private int count1, count2, count3 = 0;

    //private bool end = false;


    // Use this for initialization
    void Start()
    {
        winText.enabled = false;
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

    protected Node PickUp(GameObject p,InteractionObject ikobj)
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
        return open;
    }


    #endregion

    protected Node ST_ApproachAndWait(Transform target,GameObject participant)
    {
        Val<Vector3> position = Val.V(() => target.position);
        print(target.position.x + " " + target.position.y+target.position.z);
        return new Sequence(participant.GetComponent<BehaviorMecanim>().Node_GoTo(position), new LeafWait(1000));
    }

    protected Node ST_Approach(Vector3 target, GameObject participant)
    {
        Val<Vector3> position = Val.V(() => target);
        print(target.x + " " + target.y + " "+target.z);
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
            for (int i = 0; i < door.Length; ++i) {

                if (door[i].tag.Equals("door"))
                {
                    //print("detected" + door[0]);
                    return RunStatus.Failure; }
            }
            //print("not detected" + door[0]);
            return RunStatus.Success;
        });
    }

    public Node openTheDoor(Transform point,GameObject participant)
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
            Collider[] price = Physics.OverlapSphere(point.position, 10f);
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

    public Node WinnerCheck()
    {
        winText.enabled = true;
        return new LeafInvoke(() =>
        {
            int score1 = count1;
            int score2 = count2;
            int score3 = count3;

            if (score1 > score2 && score1 > score3)
            {
                winText.text = "Daniel1 won!";
                winner = participant;
            }
            if (score2 > score1 && score2 > score3)
            {
                winText.text = "Daniel2 won!";
                winner = participant2;
            }
            if (score3 > score1 && score3 > score2)
            {
                winText.text = "Daniel3 won!";
                winner = participant3;
            }
            return RunStatus.Success;
        });
    }

    public Node PickUpPrice(Transform point, GameObject participant)
    {
        return new LeafInvoke(() =>
        {
            Rigidbody price;
            Collider[] prices = Physics.OverlapSphere(point.position, 10f);
            for (int i = 0; i < prices.Length; ++i)
            {

                if (prices[i].tag.Equals("price"))
                {
                    print("pick");
                    price = prices[i].GetComponent<Rigidbody>();
                    ST_Approach(price.transform.position, participant);
                    PickUp(participant, prices[i].GetComponent<InteractionObject>());
                    prices[i].enabled = false;
                    count[participant] = (int)count[participant] + 1;
                    SetCountText(count[participant], count[participant2], count[participant3]);
                    print(count[participant]);
                }
                
            }
            return RunStatus.Success;
        });
    }


    protected Node BuildTreeRoot()
    {
        Node story =
            new SequenceAll(
                //ST_Approach(new Vector3(21.7f, 0.15f, 22.612f), participant),
                new Sequence(
                    
                    new SequenceShuffle(
                        
                        new Sequence(
                            this.ST_ApproachAndWait(this.point1, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.forward)),
                            new Selector(doorOpen(this.point1), this.OpenDoor(participant), openTheDoor(this.point1, participant)),
                                ST_Approach(new Vector3(40f, 0f, 37f), participant),
                            new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant))
                            ),

                        new Sequence(
                            this.ST_ApproachAndWait(this.point2, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.right)),
                            new Selector(
                                doorOpen(this.point2), this.OpenDoor(participant), openTheDoor(this.point2, participant)),
                            ST_Approach(new Vector3(20f, 0f, 10f), participant),
                            new Selector(havePrice(participant.transform), PickUpPrice(participant.transform, participant))
                        ),
                        new Sequence(
                            this.ST_ApproachAndWait(this.point3, participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.right)),
                            new Selector(doorOpen(this.point3), this.OpenDoor(participant), openTheDoor(this.point3, participant)),
                            ST_Approach(new Vector3(20f, 0f, 25f), participant),
                            new Selector(havePrice(participant.transform),PickUpPrice(participant.transform, participant))
                        )
                    ), ST_ApproachAndWait(this.point14, participant), this.WinnerCheck(), this.Cheer(winner),new LeafInvoke(() => { return RunStatus.Running; })
                 //new Vector3(39f, 0f, -12f)
                 //participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.right))

                 )
            //this.Cheer(), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.right))


            //new SequenceShuffle(
            //    //new Sequence(this.PickUp(participant), ChaChaRealSmooth(participant, 3), this.PutDown(participant)))
            //    new Sequence(
            //        this.ST_ApproachAndWait(this.point1, participant2), this.OpenDoor(participant2), new LeafWait(1000),
            //        new Selector(
            //            doorOpen(this.point1), openTheDoor(this.point1, participant2)
            //            ),
            //        this.ST_ApproachAndWait(this.point10, participant2)
            //        ),
            //    new Sequence(
            //        this.ST_ApproachAndWait(this.point2, participant2), this.OpenDoor(participant2), new LeafWait(1000),
            //        new Selector(
            //            doorOpen(this.point2), openTheDoor(this.point2, participant2)
            //            ),
            //        this.ST_ApproachAndWait(this.point10, participant2)
            //        ),
            //    new Sequence(
            //        this.ST_ApproachAndWait(this.point3, participant2), this.OpenDoor(participant2), new LeafWait(1000),
            //        new Selector(
            //            doorOpen(this.point3), openTheDoor(this.point3, participant2)
            //            ),
            //        this.ST_ApproachAndWait(this.point10, participant2)
            //        )

            //    ),
            //new SequenceShuffle(
            //    //new Sequence(this.PickUp(participant), ChaChaRealSmooth(participant, 3), this.PutDown(participant)))
            //    new Sequence(
            //        this.ST_ApproachAndWait(this.point1, participant3), this.OpenDoor(participant3), new LeafWait(1000),
            //        new Selector(
            //            doorOpen(this.point1), openTheDoor(this.point1,participant3)
            //            ),
            //        this.ST_ApproachAndWait(this.point10, participant3)
            //        ),
            //    new Sequence(
            //        this.ST_ApproachAndWait(this.point2, participant3), this.OpenDoor(participant3), new LeafWait(1000),
            //        new Selector(
            //            doorOpen(this.point2), openTheDoor(this.point2,participant3)
            //            ),
            //        this.ST_ApproachAndWait(this.point10, participant3)
            //        ),
            //    new Sequence(
            //        this.ST_ApproachAndWait(this.point3, participant3), this.OpenDoor(participant3), new LeafWait(1000),
            //        new Selector(
            //            doorOpen(this.point3), openTheDoor(this.point3,participant3)
            //            ),
            //        this.ST_ApproachAndWait(this.point10, participant3)
            //        )

            //    ),
            //new SequenceShuffle(
            //    //new Sequence(this.PickUp(participant), ChaChaRealSmooth(participant, 3), this.PutDown(participant)))
            //    new Sequence(
            //        this.ST_ApproachAndWait(this.point1, participant4), this.OpenDoor(participant4), new LeafWait(1000),
            //        new Selector(
            //            doorOpen(this.point1), openTheDoor(this.point1, participant4)
            //            ),
            //        this.ST_ApproachAndWait(this.point10, participant4)
            //        ),
            //    new Sequence(
            //        this.ST_ApproachAndWait(this.point2, participant4), this.OpenDoor(participant4), new LeafWait(1000),
            //        new Selector(
            //            doorOpen(this.point2), openTheDoor(this.point2, participant4)
            //            ),
            //        this.ST_ApproachAndWait(this.point10, participant4)
            //        ),
            //    new Sequence(
            //        this.ST_ApproachAndWait(this.point3,participant4), this.OpenDoor(participant4), new LeafWait(1000),
            //        new Selector(
            //            doorOpen(this.point3), openTheDoor(this.point3, participant4)
            //            ),
            //        this.ST_ApproachAndWait(this.point10,participant4)
            //        )
            //    )
            //new Sequence(
            //    ST_Approach(new Vector3(39f, 0f, -12f), participant), participant.GetComponent<BehaviorMecanim>().ST_TurnToFace(Val.V(() => participant.transform.position + Vector3.right)),
            //    new Selector(this.WinnerCheck(),this.Cheer(winner))
            //    )
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

    //public void Win()
    //{
    //    //GameObject winner = null;
    //    int score1 = count1;
    //    int score2 = count2;
    //    int score3 = count3;

    //    if (score1 > score2 && score1 > score3)
    //    {
    //        winText.text = "Daniel1 won!";
    //        winner = participant;
    //    }
    //    if (score2 > score1 && score2 > score3)
    //    {
    //        winText.text = "Daniel2 won!";
    //        winner = participant2;
    //    }
    //    if (score3 > score1 && score3 > score2)
    //    {
    //        winText.text = "Daniel3 won!";
    //        winner = participant3;
    //    }
    //    //if (score1 == score2)
    //    //{
    //    //    winText.text = "Daniel1 and Daniel2 tied and won!";
    //    //}
    //    //if (score1 == score3)
    //    //{
    //    //    winText.text = "Daniel1 and Daniel3 tied and won!";
    //    //}
    //    //if (score2 == score3)
    //    //{
    //    //    winText.text = "Daniel2 and Daniel3 tied and won!";
    //    //}
    //}

    public void SetCountText(object c1, object c2, object c3)
    {
        countTextP1.text = "Daniel1 Score: " + c1.ToString();
        countTextP2.text = "Daniel2 Score: " + c2.ToString();
        countTextP3.text = "Daniel3 Score: " + c3.ToString();

        count1 = (int)c1;
        count2 = (int)c2;
        count3 = (int)c3;

    }

    //private Node Cheer(GameObject p)
    //{
    //    return new Sequence(this.Node_OD(),
    //                p.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", true),
    //                new LeafWait(1000),
    //                p.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", false),
    //                new LeafInvoke(() => { return RunStatus.Failure; })
    //                );
    //}

    private Node Cheer(GameObject winner)
    {
        return new Sequence(
            //new LeafInvoke(() => {
            //    int score1 = count1;
            //    int score2 = count2;
            //    int score3 = count3;

            //    if (score1 > score2 && score1 > score3)
            //    {
            //        winText.text = "Daniel1 won!";
            //        winner = participant;
            //    }
            //    if (score2 > score1 && score2 > score3)
            //    {
            //        winText.text = "Daniel2 won!";
            //        winner = participant2;
            //    }
            //    if (score3 > score1 && score3 > score2)
            //    {
            //        winText.text = "Daniel3 won!";
            //        winner = participant3;
            //    }
            //}),
            //this.WinnerCheck(),
            winner.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER",true),
            new LeafWait(1000),
            winner.GetComponent<BehaviorMecanim>().Node_HandAnimation("CHEER", false),
            new LeafInvoke(() => { return RunStatus.Success; })
         ); 
    }

    //protected Node OpenDoor(GameObject p)
    //{

    //    return new Sequence(this.Node_OD(),
    //                        p.GetComponent<BehaviorMecanim>().Node_HandAnimation("WOODCUT", true),
    //                        new LeafWait(1000),
    //                        p.GetComponent<BehaviorMecanim>().Node_HandAnimation("WOODCUT", false),
    //                        new LeafInvoke(() => { return RunStatus.Failure; })
    //                        );

    //    //return open;
    //}

}
