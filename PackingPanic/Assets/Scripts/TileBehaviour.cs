using PackingPanick.TileData;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class TileBehaviour : InteractableObject
{
    // List of all tiles at the moment
    public static List<TileBehaviour> AllTiles = new List<TileBehaviour>();

    private Rigidbody rb;

    private InputActionAsset _inputAsset;

    private InputAction _interactAction;
    private InputAction _dropAction;

    [SerializeField]
    private GameObject _player;

    [SerializeField]
    private Slider _timer;

    private float _despawnTime;

    private bool _isHolding = false;
    private bool _isStored = false;

    // Tiledata
    private const int amountOfShapes = 7;
    TileData _tileData;

    static int amountHolding = 0;
    int maxHolding = 1;

    private bool _shouldRemoveOnDisable = false;

    bool _hasDespawned = false;

    private ParticleSystem _despawnParticle;

    public void Setup(GameObject player, InputActionAsset inputAsset, float despawnTime, ParticleSystem despawnParticle)
    {
        _despawnTime = despawnTime;
        _player = player;
        _inputAsset = inputAsset;
        _despawnParticle = despawnParticle;

        // Make sure _inputAsset is set before enabling any actions
        if (_inputAsset != null)
        {
            _interactAction = _inputAsset.FindActionMap("Gameplay").FindAction("Interact");
            _dropAction = _inputAsset.FindActionMap("Gameplay").FindAction("Drop");
        }
        else
        {
            Debug.LogError("InputActionAsset is not assigned in TileBehaviour Setup.");
        }

        if (_inputAsset != null)
        {
            _inputAsset.Enable();
            _interactAction.performed += OnInteract;
        }

        if (_timer != null)
        {
            _timer.maxValue = _despawnTime;
            _timer.minValue = 0f;
            _timer.value = _despawnTime;
        }

        // Initialize tile data after the setup
        AllTiles.Add(this);
        InitializeTileData();
    }
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnEnable()
    {
        if (_inputAsset == null) return;
        if (_inputAsset != null)
        {
            _inputAsset.Enable();
            _interactAction.performed += OnInteract;
        }

    }

    private void OnDisable()
    {
        if (_inputAsset == null) return;

        _interactAction.performed -= OnInteract; 

        // removing from list
        if(_shouldRemoveOnDisable)
            AllTiles.Remove(this);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if ((_player.transform.position - this.transform.position).magnitude < 3 && !_isStored && isHovered && !_isHolding && amountHolding < maxHolding)
        {
            _isHolding = true;
            amountHolding++;
        }
        else if(isHovered && _isHolding)
        {
            _isHolding = false;
            amountHolding--;
        }
    }

    private void Update()
    {
        Hold();
        Drop();

        if (_timer != null && !_isStored && !_isHolding)
        {
            _timer.value -= Time.deltaTime;
        }

        if(_timer != null &&_timer.value <= 0.1f ) 
        {
            if(_despawnParticle != null) 
            {
                ParticleSystem particleInstance = Instantiate(_despawnParticle, transform.position, Quaternion.identity);
                Destroy(particleInstance.gameObject, particleInstance.main.duration + particleInstance.main.startLifetime.constantMax);
            }
            
            LevelManager.RemoveTileDespawnScore();
            _hasDespawned = true;
            DisableSelf();
        }
    }

    private void Hold()
    {
        if (rb == null) return;

        
        if (_isHolding)
        {
            if (this.transform.parent == null)
            {
                this.transform.SetParent(_player.transform);
                rb.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            }

            this.transform.localPosition = Vector3.Lerp(this.transform.localPosition, Vector3.forward + (Vector3.up * 0.30f), Time.deltaTime * 5f);
            this.transform.localRotation = Quaternion.Lerp(this.transform.localRotation, Quaternion.identity, Time.deltaTime * 5f);
        }
        else if (!_isHolding && this.transform.parent == _player.transform)
        {
            this.transform.SetParent(null);

            rb.constraints = RigidbodyConstraints.None;
            rb.isKinematic = false;
        }
    }

    private void Drop()
    {
        if(_dropAction.IsPressed() && _isHolding)
        {
            _isHolding = false;
            amountHolding--;
        }
    }

    private void InitializeTileData()
    {
        _tileData.shape = TileShapeLibrary.GetShape(Random.Range(0, amountOfShapes - 1));
        Debug.Log(_tileData.shape.name);

        _tileData.color = new Color(Random.value, Random.value, Random.value); 
                                                                               
        Renderer renderer = GetComponent<Renderer>(); 
        if (renderer != null)
        {
            renderer.material.color = _tileData.color;
        }
    }

    public TileData GetTileData()
    {
        return _tileData;
    }

    public bool GetIsHolding()
    {
        return _isHolding;
    }

    public void Store(bool store)
    {
        if (!store && amountHolding >= maxHolding) return;
        _isHolding = !store;
        _isStored = store;
        if(store) 
        {
            amountHolding--;
        }
        else
        {
            amountHolding++;
        }

        this.gameObject.SetActive(!store);

        if(_timer != null)
        {
            _timer.enabled = !store;
        }
    }

    static public int GetAmountHolding()
    {
        return amountHolding;
    }

    //checks if allowed
    public void SetIsHolding(bool hold)
    { 
        if(amountHolding < maxHolding && hold && !_isHolding)
        {
            _isHolding = true;
            amountHolding++;
        }
        else if(_isHolding && !hold) 
        { 
            _isHolding=false;
            amountHolding--;
        }

    }

    private void DisableSelf()
    {
        Renderer tileRenderer = this.GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            tileRenderer.enabled = false;
        }


        Collider tileCollider = this.GetComponent<Collider>();
        if (tileCollider != null)
        {
            tileCollider.enabled = false;
        }

        if (_timer != null)
        {
            _timer.enabled = false;
        }

        gameObject.SetActive(false);
    }

    public void SetShouldRemoveOnDisable(bool shouldRemove)
    {
        _shouldRemoveOnDisable = shouldRemove;
    }

    public bool GetIsStored()
    {
        return _isStored;
    }

    public bool GetHasDespawned()
    {
        return _hasDespawned;
    }
}
