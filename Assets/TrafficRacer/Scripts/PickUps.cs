/***********************************************************************************************************
 * Produced by Madfireon:               https://www.madfireongames.com/									   *
 * Facebook:                            https://www.facebook.com/madfireon/								   *
 * Contact us:                          https://www.madfireongames.com/contact							   *
 * Madfireon Unity Asset Store catalog: http://bit.ly/sellmyapp_store									   *
 * Developed by Swapnil Rane:           https://in.linkedin.com/in/swapnilrane24                           *
 ***********************************************************************************************************/

/***********************************************************************************************************
* NOTE:- This script defiends the pickup                                                                   *
***********************************************************************************************************/

using UnityEngine;
using DG.Tweening;

public enum PickUpType { coin1, coin5, coin10, shield, doubleCoin, magnet, turboBoost }

public class PickUps : MonoBehaviour {

    [SerializeField] private PickUpType pickUpType;                 //pickup type
    [SerializeField] private float objectLength;                    //distance when travelled by object, spawns the new object
    [SerializeField] private float deactivationDistance = 35;       //distance in -ve y axis travvelled to deactive gameobject
    [SerializeField] private bool coinObj = false;

    private float randomSpeed;                                      //extra speed added to pickUp
    private float originYPos;                                       //pos at which pickUp object got active
    private bool nextObjectSpawned = false, pickedUp = false;

    public PickUpType _PickUpType { get { return pickUpType; } }                    //getter and setter
    public bool PickedUp { get { return pickedUp; } set { pickedUp = value; } }

    // Use this for initialization
    void Start ()
    {
        randomSpeed = Random.Range(2, 5);                                           //get values between 10 and 15	
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (GameManager.Instance.gameOver == true || GameManager.Instance.paused == true) return;                          //if gameover is true , then return

        if (transform.position.y <= -deactivationDistance)                          //if y position is less than - deactivation distance
        {
            switch (pickUpType)                                                     //check the pickup type
            {               
                case PickUpType.doubleCoin:
                    GuiManager.Instance.DoubleCoinSpawned = false;                  //set DoubleCoinSpawned to false
                    break;

                case PickUpType.magnet:
                    GuiManager.Instance.MagnetSpawned = false;                      //set MagnetSpawned to false

                    break;
                case PickUpType.turboBoost:
                    GuiManager.Instance.TurboSpawned = false;                       //set TurboSpawned to false
                    break;

                default:
                    break;
            }

            gameObject.SetActive(false);                                            //deactivate gameobject
        }

        //if distance coverend by the pickup is mmore than objectlength + some random value and nextObjectSpawned is false
        if (Mathf.Abs(transform.position.y - originYPos) > (objectLength + Random.Range(0, 2)) && nextObjectSpawned == false)
        {
            nextObjectSpawned = true;                                               //set nextObjectSpawned to true
            Spawner.instance.SpawnObjects();                                        //spawn the new object
        }
                                                                                    //move the pickup
        transform.Translate(-Vector3.up * Time.deltaTime * (GuiManager.Instance.CurrentSpeed + randomSpeed));

        if (GuiManager.Instance.MagnetActive && coinObj && pickedUp == false)       //if magnet is active and pickup type is coin and its not pickedup by player
        {                                                                           //check if distance between coin and player is less than 5
            if (Vector3.Distance(transform.position, GameManager.Instance.playerCar.transform.position) < 5f)
                transform.position = Vector3.MoveTowards(transform.position, GameManager.Instance.playerCar.transform.position, 25 * Time.deltaTime); ;    //move the coin towards player
        }
    }

    public void DefaultSettings()                                 //reset settings at spawning
    {
        originYPos = transform.position.y;
        nextObjectSpawned = false;
        pickedUp = false;
    }
}
