using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.SocialPlatforms.Impl;

public class LevelManager : MonoBehaviour
{
    [SerializeField]
    private int _waves = 1;

    [SerializeField]
    private int _amountOfTiles = 10;

    [SerializeField]
    private int _amountOfRequests;

    [SerializeField] // In seconds
    private float _totalTime = 100;

    [SerializeField]
    private float _clientPatience = 30;

    [SerializeField]
    private float _tileDespawnTime = 30;

    [SerializeField]
    private GameObject _player;

    [SerializeField]
    private InputActionAsset _actionAsset;

    [SerializeField]
    private GameObject _destinations;

    [SerializeField]
    private BoxCollider _clientSpawnBounds;

    [SerializeField]
    private BoxCollider[] _tileSpawnBounds;

    [SerializeField]
    private GameObject _tilePrefab;

    [SerializeField]
    private GameObject _clientPrefab;

    [SerializeField]
    static private float _tileDespawnPunishmentScore = 10;

    [SerializeField]
    static private float _clientDespawnPunishmentScore = 10;

    [SerializeField]
    float _starOneRequirementPrecentage = 30f;

    [SerializeField]
    float _starTwoRequirementPrecentage = 52f;

    [SerializeField]
    float _starThreeRequirementPrecentage = 88f;

    [SerializeField]
    private ParticleSystem _despawnParticle;

    [SerializeField]
    private ParticleSystem _correctTileParticle;

    [SerializeField]
    private ParticleSystem _wrongTileParticle;

    [SerializeField]
    private ParticleSystem _timeSlowParticle;

    [SerializeField]
    private ParticleSystem _speedBoostParticle;

    private ParticleSystem _activeSpeedBoostParticle;
    private ParticleSystem _activeTimeSlowParticle;


    [SerializeField]
    TextMeshProUGUI _boostDisplays;

    private PlayerProgress _playerProgress;

    private static float _score;

    private Vector3 _clientSpawnMin;
    private Vector3 _clientSpawnMax;

    private Vector3[] _tileSpawnMins;
    private Vector3[] _tileSpawnMaxs;

    private static int _clientsFinished = 0;

    static private bool _levelEnded = false;

    private float _maxScore = 0;

    private float _reqAmount = 0;

    private bool _speedBoostStillPlaying = false;
    private bool _timeSlowStillPlaying = false;

    private const float _speedBoostDuration = 10f;
    private const float _timeSlowDuration = 10f;

    private float _speedBoostTimePlayed;
    private float _timeSlowTimePlayed;

    InputAction _useTimeSlow;
    InputAction _useSpeedBoost;



    private void ExtractBoundsAndDisableColliders()
    {
        
        if (_clientSpawnBounds != null)
        {
            _clientSpawnMin = _clientSpawnBounds.bounds.min;
            _clientSpawnMax = _clientSpawnBounds.bounds.max;

            
            _clientSpawnBounds.enabled = false;
        }

        
        _tileSpawnMins = new Vector3[_tileSpawnBounds.Length];
        _tileSpawnMaxs = new Vector3[_tileSpawnBounds.Length];

        for (int i = 0; i < _tileSpawnBounds.Length; i++)
        {
            BoxCollider tileCollider = _tileSpawnBounds[i];
            if (tileCollider != null)
            {
                _tileSpawnMins[i] = tileCollider.bounds.min;
                _tileSpawnMaxs[i] = tileCollider.bounds.max;

                
                tileCollider.enabled = false;
            }
        }
    }

    static public void AddScore(float score)
    {
        _score += score;
    }

    static public void RemoveTileDespawnScore()
    {
        _score -= _tileDespawnPunishmentScore;
    }

    static public void RemoveClientDespawnScore()
    {
        _score -= _clientDespawnPunishmentScore;
    }

    private void Awake()
    {
        _score = 0;
        _clientsFinished = 0;
        _levelEnded = false;
        ClientBehaviour.WantedTiles.Clear();
        TileBehaviour.AllTiles.Clear();
        SceneManager.sceneLoaded += OnSceneLoaded;

        if(_actionAsset != null)
        {
            _useTimeSlow = _actionAsset.FindActionMap("Gameplay").FindAction("TimeSlow");
            _useSpeedBoost = _actionAsset.FindActionMap("Gameplay").FindAction("SpeedBoost");
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _score = 0;
        _clientsFinished = 0;
        _levelEnded = false;
        ClientBehaviour.WantedTiles.Clear();
        TileBehaviour.AllTiles.Clear();
        Time.timeScale = 1f;
    }

    private void Start()
    {
        _playerProgress = SaveLoadManager.LoadProgress();
        UpdateBoostDisplays();
        _reqAmount = _amountOfRequests;
        _maxScore = _clientPatience * (float)_amountOfRequests;
        ExtractBoundsAndDisableColliders();
        // Start the spawning process
        SpawnTilesAndClients();
    }

    private void Update()
    {
        TimeSlow();
        SpeedBoost();
    }

    private void SpawnTilesAndClients()
    {
        
        float tileInterval = _totalTime  / (float)_amountOfTiles;

        float waveInterval = _totalTime  / (float)_waves;
        int clientsPerWave = _amountOfRequests / _waves;

        StartCoroutine(SpawnTiles(tileInterval));


        StartCoroutine(SpawnClients(clientsPerWave, waveInterval));
    }

    private IEnumerator SpawnTiles(float tileInterval)
    {
        yield return new WaitForSeconds(tileInterval);

        for (int i = 0; i < _amountOfTiles; i++)
        {
            SpawnTile(); 
            yield return new WaitForSeconds(tileInterval);
        }
    }

    // Separate function for spawning clients in waves
    private IEnumerator SpawnClients(int clientsPerWave, float waveInterval)
    {

        yield return new WaitForSeconds(waveInterval);

        for (int wave = 1; wave <= _waves; wave++)
            {
                for (int i = 0; i < clientsPerWave; i++)
                {
                    SpawnClient();
                    yield return null;
                }

                yield return new WaitForSeconds(waveInterval);
            }
        
    }

    private void SpawnTile()
    {
        Vector3 spawnLocation = GetRandomLocationInBounds(_tileSpawnMins, _tileSpawnMaxs);

        GameObject tile = Instantiate(_tilePrefab, spawnLocation, Quaternion.identity);

        TileBehaviour tileScript = tile.GetComponent<TileBehaviour>();

        if(tileScript != null)
        {
            tileScript.Setup(_player, _actionAsset, _tileDespawnTime, _despawnParticle);
        }

        tile.transform.position = spawnLocation;

    }

    private void SpawnClient()
    {
        Vector3 spawnLocation = GetRandomLocationInBounds(_clientSpawnMin, _clientSpawnMax);

        GameObject client = Instantiate(_clientPrefab, spawnLocation, Quaternion.identity);

        ClientBehaviour clientScript = client.GetComponent<ClientBehaviour>();

        if(clientScript != null)
        {
            clientScript.Setup(_player, _actionAsset, _destinations, _clientPatience, _despawnParticle, _correctTileParticle, _wrongTileParticle);
        }

    }

    public Vector3 GetRandomLocationInBounds(Vector3[] vectorMins, Vector3[] vectorMaxs)
    {
        if (vectorMins == null || vectorMaxs == null || vectorMins.Length != vectorMaxs.Length || vectorMins.Length == 0)
        {
            Debug.LogWarning("VectorMin and VectorMax arrays are either empty or null, or they have different lengths.");
            return Vector3.zero;
        }

        
        int randomIndex = Random.Range(0, vectorMins.Length);

        
        Vector3 min = vectorMins[randomIndex];
        Vector3 max = vectorMaxs[randomIndex];

        return GetRandomLocationInBounds(min, max);
    }

    public Vector3 GetRandomLocationInBounds(Vector3 vectorMin, Vector3 vectorMax)
    {
        
        float x = Random.Range(vectorMin.x, vectorMax.x);
        float y = Random.Range(vectorMin.y, vectorMax.y);
        float z = Random.Range(vectorMin.z, vectorMax.z);

        return new Vector3(x, y, z);
    }

    public float GetStarRequirement(int starIndex)
    {
        switch (starIndex)
        {
            case 0:
                return _starOneRequirementPrecentage;
            case 1:
                return _starTwoRequirementPrecentage;
            case 2:
                return _starThreeRequirementPrecentage;
                
            default: return 100f;
        }

    }

    public static float GetScore()
    {
        return _score;
    }

    public static void AddFinished()
    {
        _clientsFinished += 1;
    }

    public bool GetLevelEnded()
    {
        if( _clientsFinished >= _reqAmount)
        {
            _levelEnded = true;

        }
        Debug.Log("request done: " + _clientsFinished + _amountOfRequests);
        return _levelEnded;
    }

    public float GetMaxScore()
    {
        _maxScore = _clientPatience * (float)_amountOfRequests;
        return _maxScore;
    }

    private void SpeedBoost()
    {
        if (_useSpeedBoost == null || _playerProgress == null) return;

        if (_useSpeedBoost.WasReleasedThisFrame() && !_speedBoostStillPlaying)
        {
            if (_playerProgress.amountOfSpeedBoosts > 0)
            {
                _playerProgress.amountOfSpeedBoosts--;
                SaveLoadManager.SaveProgress(_playerProgress);
                UpdateBoostDisplays();

                // Instantiate and parent the particle system
                if (_speedBoostParticle != null && _activeSpeedBoostParticle == null)
                {
                    _activeSpeedBoostParticle = Instantiate(_speedBoostParticle, _player.transform.position, Quaternion.identity);
                    _activeSpeedBoostParticle.transform.SetParent(_player.transform);

                    // Offset the particle system 1 unit backward relative to the player's transform
                    _activeSpeedBoostParticle.transform.localPosition = new Vector3(0, 0, -1);
                    _activeSpeedBoostParticle.transform.localRotation = Quaternion.Euler(0, 180, 0);
                }


                _player.GetComponent<PlayerCharacter>().SetMultiplier(1.3f);
                _speedBoostStillPlaying = true;
            }
            else
            {
                Debug.Log("No SpeedBoosts available!");
            }
        }

        if (_speedBoostStillPlaying)
        {
            _speedBoostTimePlayed += Time.deltaTime;
        }

        if (_speedBoostTimePlayed >= _speedBoostDuration)
        {
            _speedBoostTimePlayed = 0;
            _player.GetComponent<PlayerCharacter>().SetMultiplier(1f);
            _speedBoostStillPlaying = false;

            // Destroy the particle system
            if (_activeSpeedBoostParticle != null)
            {
                Destroy(_activeSpeedBoostParticle.gameObject);
                _activeSpeedBoostParticle = null;
            }
        }
    }



    private void TimeSlow()
    {
        if (_useTimeSlow == null || _playerProgress == null) return;

        if (_useTimeSlow.WasReleasedThisFrame() && !_timeSlowStillPlaying)
        {
            if (_playerProgress.amountOfTimeSlows > 0)
            {
                _playerProgress.amountOfTimeSlows--;
                SaveLoadManager.SaveProgress(_playerProgress);
                UpdateBoostDisplays();

                // Instantiate and parent the particle system
                if (_timeSlowParticle != null && _activeTimeSlowParticle == null)
                {
                    _activeTimeSlowParticle = Instantiate(_timeSlowParticle, _player.transform.position, Quaternion.identity);
                    _activeTimeSlowParticle.transform.SetParent(_player.transform);

                    // Offset the particle system 1 unit backward relative to the player's transform
                    _activeTimeSlowParticle.transform.localPosition = new Vector3(0, 0, 0);
                    _activeTimeSlowParticle.transform.localRotation = Quaternion.Euler(0, 180, 0);

                }


                _timeSlowStillPlaying = true;
            }
            else
            {
                Debug.Log("No TimeSlows available!");
            }
        }

        if (_timeSlowStillPlaying)
        {
            _timeSlowTimePlayed += Time.deltaTime;

            if (Time.timeScale > 0.2f)
            {
                Time.timeScale = 0.5f;
            }
        }

        if (_timeSlowTimePlayed >= _timeSlowDuration)
        {
            _timeSlowTimePlayed = 0;
            Time.timeScale = 1f;
            _timeSlowStillPlaying = false;

            // Destroy the particle system
            if (_activeTimeSlowParticle != null)
            {
                Destroy(_activeTimeSlowParticle.gameObject);
                _activeTimeSlowParticle = null;
            }
        }
    }


    private void UpdateBoostDisplays()
    {
        if (_boostDisplays != null && _playerProgress != null)
        {
            _boostDisplays.text = $"Speedboosts: {_playerProgress.amountOfSpeedBoosts}\nTimeslows: {_playerProgress.amountOfTimeSlows}";
        }
    }
}
