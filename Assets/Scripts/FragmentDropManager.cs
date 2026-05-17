using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

[System.Serializable]
public class FragmentData
{
    public string fragmentName;
    public Sprite fragmentSprite;
}

public class FragmentDropManager : MonoBehaviour
{
    public static FragmentDropManager instance;

    [Header("UI Components")]
    public GameObject fragmentUIPanel; // Panel have Image and Text to show fragment info
    public Image fragmentImageUI;      // Image to show fragment sprite
    public TextMeshProUGUI fragmentNameText; // Fragment name text

    [Header("Flying Effect Settings")]
    public float flySpeed = 5f;
    public float waitBeforeFly = 1f;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Start()
    {
        if (fragmentUIPanel != null) fragmentUIPanel.SetActive(false);
    }

    public void DropAndCollectFragment(FragmentData data, Vector3 spawnPos, Transform player, UnityAction onComplete)
    {
        StartCoroutine(ExecuteFragmentSequence(data, spawnPos, player, onComplete));
    }

    private IEnumerator ExecuteFragmentSequence(FragmentData data, Vector3 spawnPos, Transform player, UnityAction onComplete)
    {
        // Create the flying fragment at the spawn position
        GameObject flyingFragment = new GameObject("FlyingFragment");
        flyingFragment.transform.position = spawnPos;
        SpriteRenderer sr = flyingFragment.AddComponent<SpriteRenderer>();
        sr.sprite = data.fragmentSprite;
        sr.sortingOrder = 100; // Always on top

        // Wait a moment before starting to fly
        yield return new WaitForSeconds(waitBeforeFly);

        // Fragment flies towards the player
        while (flyingFragment != null && player != null && Vector3.Distance(flyingFragment.transform.position, player.position) > 0.5f)
        {
            flyingFragment.transform.position = Vector3.MoveTowards(flyingFragment.transform.position, player.position, flySpeed * Time.deltaTime);
            // Rotate the fragment for a nice effect
            flyingFragment.transform.Rotate(0, 0, 360 * Time.deltaTime);
            yield return null;
        }

        // Destroy the flying fragment once it reaches the player
        Destroy(flyingFragment);

        // Sound effect for collecting the fragment (optional)
        // AudioManager.instance.PlaySFX(itemCollectSound);

        // UI showing the collected fragment
        fragmentImageUI.sprite = data.fragmentSprite;
        fragmentNameText.text = data.fragmentName;
        fragmentUIPanel.SetActive(true);

        // Wait a frame to ensure UI updates before waiting for player input
        yield return new WaitForEndOfFrame();

        // Wait for player input to continue (spacebar or mouse click)
        yield return new WaitUntil(() => Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0));

        // Close the fragment UI
        fragmentUIPanel.SetActive(false);

        // Callback to notify that the sequence is complete
        onComplete?.Invoke();
    }
}