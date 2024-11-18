using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.AI;

public class ClientBehaviour : BasicCharacter
{
    public static List<TileBehaviour> WantedTiles = new List<TileBehaviour>();
    private TileBehaviour _wantedTile;

    [SerializeField]
    private Slider _timer;

    [SerializeField]
    private GameObject _player;

    [SerializeField]
    private InputActionAsset _inputAsset;

    [SerializeField]
    private GameObject _destinations;

    private InputAction _interactAction;

    [SerializeField]
    private float _patience = 10.0f;

    private Image _wantedTileImage;

    private bool _isMouseOver = false;

    private NavMeshAgent _agent;
    private DestinationVariables _currentDestination;  // Reference to the current destination
    private bool _hasDestination = false; // Boolean to track if the client has a destination

    private bool _finished = false;
    private bool _finishedApplied = false;

    private ParticleSystem _despawnParticle;
    private ParticleSystem _correctTileParticle;
    private ParticleSystem _wrongTileParticle;

    public void Setup( GameObject player, InputActionAsset inputAsset, GameObject destinations , float patience, ParticleSystem despawnParticle, ParticleSystem correctTileParticle, ParticleSystem wrongTileParticle)
    {
        _player = player;
        _inputAsset = inputAsset;
        _destinations = destinations;
        _patience = patience;
        _despawnParticle = despawnParticle;
        _wrongTileParticle = wrongTileParticle;
        _correctTileParticle = correctTileParticle;

        if (_inputAsset != null)
        {
            _interactAction = _inputAsset.FindActionMap("Gameplay").FindAction("Interact");
            _interactAction.performed += OnInteract;
        }

        if (_timer != null)
        {
            _timer.maxValue = _patience;
            _timer.minValue = 0f;
            _timer.value = _patience;
        }

        InitializeWantedTile();

    }

    protected override void Awake()
    {
        base.Awake();
        _agent = GetComponent<NavMeshAgent>();

        _timer = GetComponentInChildren<Slider>();
        if (_timer == null)
        {
            Debug.LogWarning("Timer slider not found in children.");
        }


    }


    private void Update()
    {
        CheckFinished();
        if (_finished && !_finishedApplied)
        {
            LevelManager.AddFinished();
            _finishedApplied = true;
        }

        if (_wantedTile == null)
        {
            InitializeWantedTile();  
        }

        if (_timer != null)
        {
            _timer.value -= Time.deltaTime;

            if (_timer.value <= 0.01)
            {
                if (_currentDestination != null)
                {
                    _currentDestination.SetIsTaken(false);
                }

                if (!_finishedApplied)
                {
                    LevelManager.AddFinished();
                    _finishedApplied = true;
                }

                // Spawn despawn particle above the client with scaling
                SpawnParticle(_despawnParticle, transform.position + Vector3.up, 1.5f);

                SelfDestroy(true);
            }
        }

        // Continuously check for a destination if none is set
        if (!_hasDestination)
        {
            FindAndSetDestination();
        }
        // Rotate towards counter
        if (_agent.isOnNavMesh && _hasDestination && _agent.remainingDistance <= _agent.stoppingDistance && !_agent.pathPending)
        {
            transform.rotation = Quaternion.Euler(0, 180, 0);
        }

        
    }

    private void OnMouseEnter()
    {
        _isMouseOver = true;
    }

    private void OnMouseExit()
    {
        _isMouseOver = false;
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (context.performed && _isMouseOver)
        {
            HandInTile();
        }
    }

    private void HandInTile()
    {
        float distanceToPlayer = (this.transform.position - _player.transform.position).magnitude;

        if (distanceToPlayer > 3)
            return; // Out of range

        TileBehaviour holdingTile = GetHoldingTile();

        if (holdingTile == null || holdingTile != _wantedTile) // No tile or wrong tile
        {
            SpawnParticle(_wrongTileParticle, transform.position, 1.0f);
            return;
        }

        // Correct tile logic
        LevelManager.AddScore(_timer.value);
        SpawnParticle(_correctTileParticle, transform.position, 1.0f);

        if (!_finishedApplied)
        {
            LevelManager.AddFinished();
            _finishedApplied = true;
        }

        if (_wantedTile != null)
        {
            _wantedTile.SetIsHolding(false);
            _wantedTile.SetShouldRemoveOnDisable(true);

            Renderer renderer = _wantedTile.gameObject.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.enabled = false;
            }

            Collider collider = _wantedTile.gameObject.GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = false;
            }

            Canvas timer = _wantedTile.gameObject.GetComponent<Canvas>();
            if (timer != null)
            {
                timer.enabled = false;
            }
        }

        if (_currentDestination != null)
        {
            _currentDestination.SetIsTaken(false);
        }

        SelfDestroy(false);
    }


    public void FindWantedTile() //Recursive function that looks for a tile in the list of tiles and then looks ifits wanted already or not
    { 
        if (TileBehaviour.AllTiles.Count <= 0)
        {
            return;
        }

        int tileIndex = Random.Range(0, TileBehaviour.AllTiles.Count);
        TileBehaviour candidateTile = TileBehaviour.AllTiles[tileIndex];

        if (WantedTiles.Count >= TileBehaviour.AllTiles.Count)
        {
            return;
        }

        if (!WantedTiles.Contains(candidateTile))
        {
            WantedTiles.Add(candidateTile);
            _wantedTile = candidateTile;
            Debug.Log("Found new wanted tile: " + _wantedTile);
        }
        else
        {
            FindWantedTile();
        }
    }

    private TileBehaviour GetHoldingTile()
    {
        List<TileBehaviour> tiles = TileBehaviour.AllTiles;

        for (int i = 0; i < tiles.Count; i++)
        {
            if (tiles[i].GetIsHolding()) return tiles[i];
        }

        return null;
    }

    private void FindAndSetDestination()
    {
        foreach (Transform child in _destinations.transform)
        {
            DestinationVariables destination = child.GetComponent<DestinationVariables>();
            if (destination != null && !destination.GetIsTaken())
            {
                destination.SetIsTaken(true);
                _currentDestination = destination;
                _hasDestination = true; 

                
                _agent.SetDestination(child.position);
                break;
            }
        }
    }

    private void InitializeWantedTile()
    {
        FindWantedTile();
        if (_wantedTile != null)
        {
            Sprite tilePreview = TileSpriteGenerator.CreateTileSprite(_wantedTile.GetTileData().shape, _wantedTile.GetTileData().color);

            _wantedTileImage = transform.Find("Canvas/WantedTile").GetComponent<Image>();

            if (_wantedTileImage != null)
            {
                _wantedTileImage.sprite = tilePreview;
            }
            else
            {
                Debug.LogWarning("WantedTile Image component not found.");
            }
        }
    }

    private void SelfDestroy(bool isTileFreeAgain)
    {
        if(isTileFreeAgain && _wantedTile != null) 
        {
            WantedTiles.Remove(_wantedTile);
        }

        if (_interactAction != null)
        {
            _interactAction.performed -= OnInteract;
        }

        Destroy(this.gameObject);
    }

    private void CheckFinished()
    {
        if(_wantedTile != null && _wantedTile.GetHasDespawned())
        {
            _finished = true;
        }
    }

    private void SpawnParticle(ParticleSystem particlePrefab, Vector3 position, float scaleMultiplier)
    {
        if (particlePrefab == null) return;

        // Instantiate the particle system at the given position
        ParticleSystem particleInstance = Instantiate(particlePrefab, position, Quaternion.identity);

       
        particleInstance.transform.localScale *= scaleMultiplier;
        Destroy(particleInstance.gameObject, particleInstance.main.duration + particleInstance.main.startLifetime.constantMax);
    }

}
