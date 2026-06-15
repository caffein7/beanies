using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Moveset : MonoBehaviour
{
    [SerializeField] public Transform enemy; // Reference to the enemy's transform, assigned in the Unity Editor.
    private float horizontal;
    private float speed = 8f;
    private float jumpingPower = 16f;

    // SerializeField allows us to assign these references in the Unity Editor, even though they are private.
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private LayerMask groundLayer;

    // Move damage values
    public float jab = 10f; //done
    public float cross = 10f; //done
    public float kick = 15f; //done
    public float uppercut = 20f; //done
    public float bodyHook = 20f; //done
    public float combo = 25f; //done
    public float pushKick = 15f; //done 
    public float roundhouseKick = 30f; //done 
    public float knee = 20f; // done
    public float lowKick = 15f; // done 
    public float supermanPunch = 30f;

    public float attackRange = 10f; // Range within which the attack can hit an opponent
    public float attackDuration = 0.5f; // Duration of the attack

    // Combo buffering window (seconds). Pressed keys within this window count as a combo.
    [SerializeField] private float comboWindow = 0.25f;

    private float lastJTime = -Mathf.Infinity;
    private float lastITime = -Mathf.Infinity;
    private float lastSTime = -Mathf.Infinity; // buffer S presses for combos (used for J+S and I+S)
    private float lastATime = -Mathf.Infinity; // buffer A presses for A+K / A+O moves
    private float lastDTime = -Mathf.Infinity; // buffer D presses for D+I superman punch

    // Wrapper property that exposes rb.velocity as "linearVelocity".
    // Safely handles the case where rb is not assigned (returns Vector2.zero).
    private Vector2 linearVelocity
    {
        get => rb != null ? rb.linearVelocity : Vector2.zero;
        set { if (rb != null) rb.linearVelocity = value; }
    }

    private void Awake()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();
    }

    private void ExecuteJab()
    {
        Debug.Log("Jab executed! Damage: " + jab);
        // future: add hit detection / damage application
    }

    private void ExecuteCross()
    {
        Debug.Log("Cross executed! Damage: " + cross);
        // future: add hit detection / damage application
    }

    private void ExecuteCombo()
    {
        Debug.Log("Combo executed! Damage: " + combo);
        // future: add combo animation/state handling
    }

    private void ExecuteKick()
    {
        Debug.Log("Kick executed! Damage: " + kick);
    }

    private void ExecuteUppercut()
    {
        Debug.Log("Uppercut executed! Damage: " + uppercut);
    }

    private void ExecuteBodyHook()
    {
        Debug.Log("Hook executed! Damage: " + bodyHook);
        // future: add hit detection, animations, etc.
    }

    private void ExecutePushKick()
    {
        Debug.Log("Push Kick executed! Damage: " + pushKick);
        // future: add push effect / hit detection
    }

    private void ExecuteRoundhouse()
    {
        Debug.Log("Roundhouse Kick executed! Damage: " + roundhouseKick);
        // future: add hit detection, animation, etc.
    }

    private void ExecuteKnee()
    {
        Debug.Log("Knee executed! Damage: " + knee);
        // future: add hit detection, knockback, animation, etc.
    }

    private void ExecuteLowKick()
    {
        Debug.Log("Low Kick executed! Damage: " + lowKick);
        // future: add low-hit detection, animation, etc.
    }

    private void ExecuteSupermanPunch()
    {
        Debug.Log("Superman Punch executed! Damage: " + supermanPunch);
        // future: add forward lunge effect, hit detection, animation, etc.
    }

    void Update()
    {
        // Get horizontal input from the player (A/D keys or Left/Right arrows).
        horizontal = Input.GetAxisRaw("Horizontal");

        if (Input.GetButtonDown("Jump") && IsGrounded()) // If the player presses the jump button and is grounded, set the vertical velocity to the jumping power.
        {
            linearVelocity = new Vector2(linearVelocity.x, jumpingPower);
        }

        if (Input.GetButtonUp("Jump") && linearVelocity.y > 0f) // Variable jump height
        {
            linearVelocity = new Vector2(linearVelocity.x, linearVelocity.y * 0.5f);
        }

        // Only allow attacks when enemy is within range
        bool inRange = enemy != null && Vector3.Distance(transform.position, enemy.position) <= attackRange;
        if (!inRange)
            return;

        // Input handling with buffering so you don't need to press both buttons the same frame.
        if (Input.GetKeyDown(KeyCode.J))
        {
            lastJTime = Time.time;
            ExecuteJab();

            // If I was pressed recently, treat as combo (order-insensitive)
            if (Time.time - lastITime <= comboWindow)
                ExecuteCombo();
        }

        if (Input.GetKeyDown(KeyCode.I))
        {
            lastITime = Time.time;

            // Superman punch: D held or D pressed recently
            if (Input.GetKey(KeyCode.D) || Time.time - lastDTime <= comboWindow)
            {
                ExecuteSupermanPunch();
            }
            else
            {
                ExecuteCross();

                // If J was pressed recently, treat as combo (order-insensitive)
                if (Time.time - lastJTime <= comboWindow)
                    ExecuteCombo();

                // If S was pressed recently, treat as hook (I + S)
                if (Time.time - lastSTime <= comboWindow)
                    ExecuteBodyHook();
            }
        }

        // Handle D press (buffer for D+I superman punch). D is also used for movement; we only record the press.
        if (Input.GetKeyDown(KeyCode.D))
        {
            lastDTime = Time.time;
        }

        // Handle A press (buffer for A+K / A+O special moves). A is also used for movement; we only record the press.
        if (Input.GetKeyDown(KeyCode.A))
        {
            lastATime = Time.time;
        }

        // K key handling: check low-kick (S held) first, then A+K pushKick, otherwise regular Kick.
        if (Input.GetKeyDown(KeyCode.K))
        {
            if (Input.GetKey(KeyCode.S))
            {
                ExecuteLowKick(); // S held + K => Low Kick
            }
            else if (Time.time - lastATime <= comboWindow)
            {
                ExecutePushKick(); // A then K within window => Push Kick
            }
            else
            {
                ExecuteKick(); // single K => regular Kick
            }
        }

        // O key handling: check low-kick (S held) first, then knee (W + D held), then A+O roundhouse, otherwise regular kick.
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (Input.GetKey(KeyCode.S))
            {
                ExecuteLowKick(); // S held + O => Low Kick
            }
            else if (Input.GetKey(KeyCode.W) && Input.GetKey(KeyCode.D))
            {
                ExecuteKnee();
            }
            else if (Time.time - lastATime <= comboWindow)
            {
                ExecuteRoundhouse(); // A then O within window => Roundhouse
            }
            else
            {
                ExecuteKick(); // single O => regular Kick
            }
        }

        // S key: can be used as modifier for combos
        if (Input.GetKeyDown(KeyCode.S))
        {
            lastSTime = Time.time;

            // Uppercut: J then S within window
            if (Time.time - lastJTime <= comboWindow)
                ExecuteUppercut();

            // Hook: I then S within window
            if (Time.time - lastITime <= comboWindow)
                ExecuteBodyHook();
        }
    }

    private void FixedUpdate()
    {
        // Apply horizontal movement based on player input and speed.
        linearVelocity = new Vector2(horizontal * speed, linearVelocity.y);
    }

    private bool IsGrounded()
    {
        // Check if the player is touching the ground by using a circle overlap at the groundCheck position.
        if (groundCheck == null)
            return false;
        return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer) != null;
    }
}