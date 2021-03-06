﻿/***********************************************************************************************************
 * Produced by Madfireon:               https://www.madfireongames.com/									   *
 * Facebook:                            https://www.facebook.com/madfireon/								   *
 * Contact us:                          https://www.madfireongames.com/contact							   *
 * Madfireon Unity Asset Store catalog: http://bit.ly/sellmyapp_store									   *
 * Developed by Swapnil Rane:           https://in.linkedin.com/in/swapnilrane24                           *
 ***********************************************************************************************************/

/***********************************************************************************************************
* NOTE:- This script controls player car                                                                   *
***********************************************************************************************************/

using UnityEngine;
using DG.Tweening;
using UnityEngine.EventSystems;
using DG.Tweening;

public class PlayerController : MonoBehaviour {

    public static PlayerController instance;

    public enum MovementType { normal, laneSnap }
    [SerializeField] private Ease           ease = Ease.InBack;                                 //ease variable
    [SerializeField] private MovementType   movementType = MovementType.normal;                 //movement type
    [SerializeField] private GameObject     body, magnetEffect, turboFireEffect, smokeEffect, shield;   //ref to gameobjects
    [SerializeField] private float          xSpeed, moveLimit, steerRotationSpeed;              //speed of rotation
    [SerializeField] private int            maxSteerRotation;                                   //max rotation car can rotate
    [SerializeField] private SpriteRenderer spriteRenderer;                                     //ref to SpriteRenderer
    [SerializeField] private Sprite[]       sprites;                                            //ref to all car sprites
	[SerializeField] private Sprite[] boosterSprites;                                            //ref to all car sprites

	[SerializeField] private GameObject leftPadle;
	[SerializeField] private GameObject rightPadle;
	[SerializeField] private GameObject boosterPadles;
	[SerializeField] private Animator animator;


	private Vector3 currentPos, lastPos;                                            //ref to current position and last position if car
    private float minMoveDistanceToRotate = 0.02f;                                  //minimum distance to be moved by car to rotate
    private float rotation;                                                         //store the rotation angle
    private int currentLane = 0;                                                    //store reference to current lane player car is on
    private bool playerMoving = false, playerMoved = false, invinsible = false;     //bool variables
    float newPos = 0, x;                                                            //for lane snapping movement
    private bool leftPressed, rightPressed;                                         //bool to tell which screen side is pressed

    // Use this for initialization
    void Awake ()
    {
        if (instance == null) instance = this;
	}

    public void SetCarSprite()
    {
        spriteRenderer.sprite = sprites[GameManager.Instance.selectedCar];          //set the car sprite, depending on selected car
    }
	
	// Update is called once per frame
	void Update ()
    {                                                                               //if gameover is true or game started is false , return

        if (GameManager.Instance.gameOver == true || GameManager.Instance.gameStarted == false || GameManager.Instance.paused == true) return;

        if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())  //if mouse button is cliked is no UI is clicked
            GetMouseInput();                                                        // call GetMouseInput method
        else if (Input.GetMouseButtonUp(0))                                         // is mouse click is released
            ResetInputs();                                                          //Reset Inputs

        //if (movementType == MovementType.normal)                                    //if movement is normal type
        //{
        //    Rotation();                                                             //rotation if car
        //    Movement();                                                             //movement if car
        //}
        //else if (movementType == MovementType.laneSnap)                             //if movement is laneSnap type
        //{
            SnapMovement();                                                         //snap movement
        //}
    }

    void SnapMovement()
    {
        if (playerMoving == false)                                                  //if playerMoving is false
        {
            if (rightPressed == true/*Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow) || */)    //we check the input
            {
                if (currentLane < 2) currentLane++;                                 //update the currentLane
                playerMoving = true;                                                //set playerMoving to true
                rightPressed = false;                                               //rightPressed to false
            }

            if (leftPressed == true)//Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow) || )      //we check the input
			{
			    if (currentLane > -2) currentLane--;                                //update the currentLane
			    playerMoving = true;                                                //set playerMoving to true
			    leftPressed = false;                                                //leftPressed to false
			}


			if      (currentLane == -1) newPos = -1.2f;                             //depending on currentLane we set the newPos
            else if (currentLane == -2) newPos = -2.4f;
            else if (currentLane ==  0) newPos = 0;
            else if (currentLane ==  1) newPos = 1.2f;
            else if (currentLane ==  2) newPos = 2.4f;
        }

        transform.DOMoveX(newPos, 0.5f).OnComplete(MovementComplete).SetEase(ease); //move the player
    }

    void GetMouseInput()                                                            //get the mouse input
    {
        if (Input.mousePosition.x < Screen.width / 2) MoveLeft();                   //if we tap on left side of screen, call MoveLeft method
        else if (Input.mousePosition.x > Screen.width / 2) MoveRight();             //if we tap on right side of screen, call MoveRight method
    }

    void ResetInputs()                                                              //reset inputs
    {
		leftPressed = false;                                                    //reset the bools
		rightPressed = false;
		if (movementType == MovementType.laneSnap)                                  //if movementType is laneSnap
        {   

        }   
        else if (movementType == MovementType.normal)                               //if movementType is normal
            x = 0;                                                                  //set x to 0
    }   

    void MoveLeft()                                                                 //MoveLeft method
    {
        if (movementType == MovementType.laneSnap)                                  //if movementType is laneSnap
            leftPressed = true;                                                     //leftPressed is true
        else if (movementType == MovementType.normal)                               //if movementType is normal
            x = -1;                                                                 //x is -1, this is for touch inputs
    }

    void MoveRight()                                                                //MoveRight method
    {
        if (movementType == MovementType.laneSnap)                                  //if movementType is laneSnap
            rightPressed = true;                                                    //rightPressed is true
        else if (movementType == MovementType.normal)                               //if movementType is normal
            x = 1;                                                                  //x is 1, this is for touch inputs
    }   

    void MovementComplete()                                                         
    {
        playerMoving = false;
    }

    void Movement()                                                                 //Movement method
    {
        x = Input.GetAxis("Horizontal");                                            //set x to Horizontal input, this is for keyboard inputs
        transform.Translate(x * Vector3.right * xSpeed * Time.deltaTime);
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -moveLimit, moveLimit), transform.position.y, 0);
    }

    void Rotation()
    {
        currentPos = transform.position;                                            //store the position of car in currentPos variable
                                                                                    //check if distance between currentPos and last pos is more than minMoveDistanceToRotate
        if (Vector3.Distance(currentPos, lastPos) > minMoveDistanceToRotate)
        {
            if (currentPos.x > lastPos.x)                                           //we check if currentPos x value is more than lastPos x value
                rotation += Time.deltaTime * steerRotationSpeed;                    //rotate the car body in right side
            else if (currentPos.x < lastPos.x)                                      //we check if currentPos x value is less than lastPos x value
                rotation += Time.deltaTime * -steerRotationSpeed;                   //rotate the car body in left side
        }
        //if distance between currentPos and last pos is not more than minMoveDistanceToRotate
        else
        {
            if (rotation > 0)                                                       //and rotation is more than zero
            {
                rotation += Time.deltaTime * -steerRotationSpeed * 2;               //we reduce the rotation
                if (rotation < 0)                                                   //when its less than zero
                    rotation = 0;                                                   //we set it zero
            }
            else if (rotation < 0)                                                  //if rotation is less than zero
            {
                rotation += Time.deltaTime * steerRotationSpeed * 2;                //we increase it
                if (rotation > 0)                                                   //when it goes more than zero
                    rotation = 0;                                                   //we set it zero
            }
        }

        lastPos = currentPos;                                                       //set lastPos = currentPos

        rotation = Mathf.Clamp(rotation, -maxSteerRotation, maxSteerRotation);      //clamp the rotation between max and min SteerRotation
        body.transform.localEulerAngles = new Vector3(0, 0, -rotation);             //apply the rotation to model
    }

    private void OnTriggerEnter2D(Collider2D other)                                 //call when player detect any trigger
    {   
        if (other.CompareTag("Fuel"))                                               //check if tag is Fuel
        {
            GuiManager.Instance.PickUpPop("Re-Fueled");                             //show refuel pop up
            SoundManager.instance.CarFX("Fuel");                                    //play the sound
            GuiManager.Instance.RestoreFuel();                                      //refuel the car
            other.gameObject.SetActive(false);                                      //deactivate the gameobject
        }

        if (other.CompareTag("Enemy"))                                              //check if tag is Enemy
        {
            if (invinsible == false)                                                //check if player is not invinsible
            {
                if (GuiManager.Instance.ShieldActive == true)                       //check if player shield is on
                {
                    GuiManager.Instance.ShieldActive = false;                       //set shielded to false
                    shield.SetActive(false);                                        //deactivate player shield image
                    SoundManager.instance.CarFX("Explosion");                           //explosion sound
                    ObjectPooling.instance.SpawnPickUpFX("ExplosionFX", transform.position, other.GetComponent<TrafficCar>().RandomSpeed);   //spawn explosion effect
                }
                else
                {
                    SoundManager.instance.PlayNarrationFX("GameOver");                  //play gameover sound
                    SoundManager.instance.CarFX("Explosion");                           //car explosion sound
                    GameManager.Instance.gameOver = true;                               //gameover is true
                    other.gameObject.SetActive(false);                                  //deactivate the gameobject
                    GuiManager.Instance.GameOverMethod();                               //call GameoverMethod
                    ObjectPooling.instance.SpawnPickUpFX("ExplosionFX", transform.position, other.GetComponent<TrafficCar>().RandomSpeed);   //spawn explosion effect
                    gameObject.SetActive(false);                                        //deactivate gamobject
                }
            }
            else
            {
                SoundManager.instance.CarFX("Explosion");                           //explosion sound
                ObjectPooling.instance.SpawnPickUpFX("ExplosionFX", transform.position, other.GetComponent<TrafficCar>().RandomSpeed);   //spawn explosion effect
            }

            other.gameObject.SetActive(false);
        }

        if (other.CompareTag("PickUp"))                                             //check if tag is PickUp
        {
            PickUps pickUps = other.GetComponent<PickUps>();                        //get reference to PickUp script
            PickUpType pickUpType = pickUps._PickUpType;                            //check pickup type
            switch (pickUpType)
            {   
                case PickUpType.coin1:                                                          //if its coin1  
                    ObjectPooling.instance.SpawnPickUpFX("CoinPickUpFX", transform.position);   //spawn coin effect
                    SoundManager.instance.CarFX("Coin");                                        //play coin sound
                    other.GetComponent<PickUps>().PickedUp = true;                              //coin is picked up
                    GuiManager.Instance.IncreaseCoin(1,other.gameObject);                       //increase coins
                    break;  
                case PickUpType.coin5:                                                          //if its coin5 
                    ObjectPooling.instance.SpawnPickUpFX("CoinPickUpFX", transform.position);
                    SoundManager.instance.CarFX("Coin");
                    other.GetComponent<PickUps>().PickedUp = true;
                    GuiManager.Instance.IncreaseCoin(5, other.gameObject);
                    break;
                case PickUpType.coin10:
                    ObjectPooling.instance.SpawnPickUpFX("CoinPickUpFX", transform.position);
                    SoundManager.instance.CarFX("Coin");
                    other.GetComponent<PickUps>().PickedUp = true;
                    GuiManager.Instance.IncreaseCoin(10, other.gameObject);
                    break;
                case PickUpType.shield:                                                         //if its shield powerup
                    SoundManager.instance.PlayNarrationFX("ShieldActive");                      //play shield sound
                    GuiManager.Instance.ShieldActive = true;                                    //set shieldActive to true
                    shield.transform.DOScale(5f, 0f);                                           //set the shield scale to 5
                    shield.GetComponent<SpriteRenderer>().DOFade(0, 0f);                        //set the shield alpha to 0
                    shield.SetActive(true);
                    shield.GetComponent<SpriteRenderer>().DOFade(1, 0.25f);
                    shield.transform.DOScale(1.3f, 0.25f);
                    GuiManager.Instance.PickUpPop("Shield Activated");                          //play popup of shield is activated
                    other.gameObject.SetActive(false);                                          //deactivate gameobject
                    break;
                case PickUpType.doubleCoin:                                                     //if its doubleCoin powerup
                    SoundManager.instance.CarFX("Coin");
                    GuiManager.Instance.ActivateDoubleCoin();                                   //double coin activated
                    GuiManager.Instance.PickUpPop("Double Coin Activated");                     //play popup of doubleCoin is activated
                    other.gameObject.SetActive(false);                                          //deactivate gameobject
                    GuiManager.Instance.DoubleCoinSpawned = false;                              //set to false
                    break;
                case PickUpType.magnet:                                                         //if its magnet powerup
                    magnetEffect.SetActive(true);                                               //activate magnet effect
                    SoundManager.instance.CarFX("MagnetPickUpFx");                              //play sound effect
                    GuiManager.Instance.ActivateMagnet();                                       //magnet activated
                    GuiManager.Instance.PickUpPop("Magnet Activated");                          //play popup of Magnet is activated
                    other.gameObject.SetActive(false);                                          //deactivate gameobject
                    GuiManager.Instance.MagnetSpawned = false;                                  //set to false
                    break;
                case PickUpType.turboBoost:                                                     //if its turboBoost powerup
                    invinsible = true;
                    SoundManager.instance.PlayNarrationFX("TurboOn");                           //play sound effect
                    smokeEffect.SetActive(false);                                               //deactivate smoke effect
                    turboFireEffect.SetActive(true);                                            //activate turboBoost effect
                    GuiManager.Instance.ActivateTurbo();                                        //turboBoost activated
                    GuiManager.Instance.PickUpPop("Turbo Boost Activated");                     //play popup of turboBoost is activated
                    other.gameObject.SetActive(false);                                          //deactivate gameobject
                    GuiManager.Instance.TurboSpawned = false;                                   //set to false
					leftPadle.SetActive(false);
					rightPadle.SetActive(false);
					if(boosterSprites[GameManager.Instance.selectedCar] != null)
						spriteRenderer.sprite = boosterSprites[GameManager.Instance.selectedCar];
					animator.Play("Boost", -1, 0);
					boosterPadles.SetActive(true);
					break;
                default:
                    break;
            } 
        }
    }

    public void DeactivateEffects(string value)
    {
        if (value == "magnet")
            magnetEffect.SetActive(false);
        else if (value == "turbo")
        {
            invinsible = false;
            smokeEffect.SetActive(true);
            turboFireEffect.SetActive(false);
			leftPadle.SetActive(true);
			rightPadle.SetActive(true);
			if (sprites[GameManager.Instance.selectedCar] != null)
				spriteRenderer.sprite = sprites[GameManager.Instance.selectedCar];
			animator.Play("Pedals", -1, 0);
			boosterPadles.SetActive(false);

		}
    }

	public void PauseAnimation()
	{
		animator.speed = 0f;
	}

	public void ResumeAnimation()
	{
		animator.speed = 1f;
	}

}