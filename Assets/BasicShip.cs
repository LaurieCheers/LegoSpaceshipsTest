using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class field<T>
{
    T baseValue;
    public T value { get; private set; }
    List<Modifier<T>> modifiers = new List<Modifier<T>>();

    public field(T baseValue)
    {
        this.baseValue = baseValue;
        Recalculate();
    }

    public void BeginModifier(Modifier<T> mod)
    {
        mod.affecting = this;
        modifiers.Add(mod);
        Recalculate();
    }

    public void EndModifier(Modifier<T> effect)
    {
        effect.affecting = null;
        modifiers.Remove(effect);
        Recalculate();
    }

    public void ClearModifiers()
    {
        modifiers.Clear();
        Recalculate();
    }

    void Recalculate()
    {
        value = baseValue;
        foreach (Modifier<T> mod in modifiers)
        {
            value = mod.Apply(value);
        }
    }

    public IEffect Setter(T newValue)
    {
        return new Effect<T>(this, new List<Modifier<T>> { new Modifier_Set<T>(newValue) });
    }
}

class float_field: field<float>
{
    public float_field(float baseValue): base(baseValue) { }

    public static IEffect operator+(float_field property, float add)
    {
        return new Effect<float>(property, new List<Modifier<float>> { new Modifier_Add_float(add) });
    }
}

class int_field : field<int>
{
    public int_field(int baseValue) : base(baseValue) { }

    public static IEffect operator +(int_field property, int add)
    {
        return new Effect<int>(property, new List<Modifier<int>> { new Modifier_Add_int(add) });
    }
}

class bool_field : field<bool>
{
    public bool_field(bool baseValue) : base(baseValue) { }
}

public abstract class Modifier<T>
{
    field<T> _affecting;
    public field<T> affecting { get { return _affecting; } set
        {
            if (_affecting != value)
            {
                if (_affecting != null)
                {
                    field<T> oldAffected = _affecting;
                    _affecting = null;
                    oldAffected.EndModifier(this);
                }

                _affecting = value;

                if (value != null)
                {
                    value.BeginModifier(this);
                }
            }
        }
    }

    public abstract T Apply(T value);
}

public class Modifier_Add_float : Modifier<float>
{
    float add;

    public Modifier_Add_float(float add)
    {
        this.add = add;
    }

    public override float Apply(float value)
    {
        return value + add;
    }
}

public class Modifier_Add_int: Modifier<int>
{
    int add;

    public Modifier_Add_int(int add)
    {
        this.add = add;
    }

    public override int Apply(int value)
    {
        return value + add;
    }
}

public class Modifier_Multiply : Modifier<float>
{
    float mul;

    public Modifier_Multiply(float mul)
    {
        this.mul = mul;
    }

    public override float Apply(float value)
    {
        return value * mul;
    }
}

public class Modifier_Set<T> : Modifier<T>
{
    T newValue;

    public Modifier_Set(T newValue)
    {
        this.newValue = newValue;
    }

    public override T Apply(T value)
    {
        return newValue;
    }
}

abstract class Button
{
    public abstract bool held { get; }
    public abstract bool justPressed { get; }
    public abstract bool justReleased { get; }
}

class Button_Mouse: Button
{
    public int button;

    public Button_Mouse(int button)
    {
        this.button = button;
    }

    public override bool held { get { return Input.GetMouseButton(button); } }
    public override bool justPressed { get { return Input.GetMouseButtonDown(button); } }
    public override bool justReleased { get { return Input.GetMouseButtonUp(button); } }
}

class Button_Key: Button
{
    public KeyCode keyCode;

    public Button_Key(KeyCode keyCode)
    {
        this.keyCode = keyCode;
    }

    public override bool held { get { return Input.GetKey(keyCode); } }
    public override bool justPressed { get { return Input.GetKeyDown(keyCode); } }
    public override bool justReleased { get { return Input.GetKeyUp(keyCode); } }
}

abstract class ShipModule
{
    public Button button;

    public ShipModule(Button button, BasicShip ship)
    {
        this.button = button;
        this.ship = ship;
    }
    
    public BasicShip ship { get; private set; }
    public virtual void Start() { }
    public virtual void Update(Vector3 mousePos) { }
    public virtual void FixedUpdate(Vector3 mousePos) { }
}

class ShipModule_Beam: ShipModule
{
    public LineRenderer qBeam;
    public readonly float_field damage;

    public ShipModule_Beam(LineRenderer beam, float baseDamage, Button button, BasicShip ship): base(button, ship)
    {
        this.qBeam = beam;
        damage = new float_field(baseDamage);
    }

    public override void Start()
    {
        qBeam.SetPositions(new Vector3[] { Vector3.zero, Vector3.zero });
        qBeam.enabled = false;
    }

    public override void FixedUpdate(Vector3 mousePos)
    {
        if(button.held)
        {
            Vector3 mouseOffset = mousePos - ship.transform.position;
            Vector3 beamDirection = mouseOffset;
            beamDirection.z = 0;
            beamDirection.Normalize();
            Vector3 endPoint = ship.transform.position + beamDirection * 100;
            RaycastHit2D hit = Physics2D.Linecast(ship.transform.position + beamDirection * 0.5f, endPoint);
            if (hit.collider != null)
            {
                HealthComponent targetHealth = hit.collider.gameObject.GetComponent<HealthComponent>();
                if (targetHealth != null)
                {
                    targetHealth.TakeDamage(damage.value);
                }
                qBeam.SetPositions(new Vector3[] { ship.transform.position, hit.point });
                qBeam.enabled = true;
            }
            else
            {
                qBeam.SetPositions(new Vector3[] { ship.transform.position, endPoint });
                qBeam.enabled = true;
            }
        }
        else
        {
            qBeam.enabled = false;
        }
    }
}

class ShipModule_Projectile : ShipModule
{
    public GameObject bulletPrefab;
    public readonly float_field speed;
    public readonly float_field range;
    public readonly float_field damage;
    public readonly float_field spread;
    public readonly int_field bulletsPerShot;
    public readonly bool_field automatic;
    public readonly float_field firingRate;

    float nextShootTime;

    public ShipModule_Projectile(GameObject bulletPrefab, float baseSpeed, float baseRange, float baseDamage, float baseSpread, int bulletsPerShot, float baseFiringRate, bool automatic, Button button, BasicShip ship) : base(button, ship)
    {
        this.bulletPrefab = bulletPrefab;
        this.speed = new float_field(baseSpeed);
        this.range = new float_field(baseRange);
        this.damage = new float_field(baseDamage);
        this.spread = new float_field(baseSpread);
        this.bulletsPerShot = new int_field(bulletsPerShot);
        this.firingRate = new float_field(baseFiringRate); 
        this.automatic = new bool_field(automatic);
    }

    public override void Update(Vector3 mousePos)
    {
        if (nextShootTime > Time.time)
            return;

        bool shouldShoot = (automatic.value) ? button.held : button.justPressed;

        if (shouldShoot)
        {
            nextShootTime = Time.time + 1/firingRate.value;
            float currentAngle = ship.transform.rotation.eulerAngles.z - (bulletsPerShot.value-1) * spread.value/2;
            for (int Idx = 0; Idx < bulletsPerShot.value; ++Idx)
            {
                Bullet newBullet = GameObject.Instantiate(bulletPrefab).GetComponent<Bullet>();
                newBullet.transform.position = ship.transform.position;
                Vector3 shotDirection = new Vector3(Mathf.Sin(-currentAngle * Mathf.Deg2Rad), Mathf.Cos(-currentAngle * Mathf.Deg2Rad), 0);
                newBullet.velocity = ship.velocity + shotDirection * speed.value;
                newBullet.transform.rotation = Quaternion.LookRotation(Vector3.forward, newBullet.velocity);
                newBullet.DieAtTimestamp = Time.time + range.value * Time.fixedDeltaTime / speed.value;
                newBullet.damage = damage.value;

                newBullet.source = ship.gameObject;

                currentAngle += spread.value;
            }
        }
    }
}

public interface IEffect
{
    void Begin();
    void End();
}

class Effect<T>: IEffect
{
    field<T> property;
    List<Modifier<T>> modifiers;

    public Effect(field<T> property, List<Modifier<T>> modifiers)
    {
        this.property = property;
        this.modifiers = modifiers;
    }
    
    public void Begin()
    {
        foreach (Modifier<T> mod in modifiers)
        {
            property.BeginModifier(mod);
        }
    }

    public void End()
    {
        foreach (Modifier<T> mod in modifiers)
        {
            property.EndModifier(mod);
        }
    }
}

class ShipModule_TimedSwitch : ShipModule
{
    List<IEffect> effects;
    float duration;
    float endAtTimestamp;

    public ShipModule_TimedSwitch(float duration, Button button, BasicShip ship, List<IEffect> effects) : base(button, ship)
    {
        this.duration = duration;
        this.effects = effects;
    }

    public override void Update(Vector3 mousePos)
    {
        if(endAtTimestamp != 0 && endAtTimestamp < Time.time)
        {
            foreach (IEffect effect in effects)
            {
                effect.End();
            }
            endAtTimestamp = 0;
        }

        if (button.justPressed && endAtTimestamp == 0)
        {
            foreach (IEffect effect in effects)
            {
                effect.Begin();
            }
            endAtTimestamp = Time.time + duration;
        }
    }
}

public class BasicShip : MonoBehaviour
{
    public LineRenderer qBeam;
    public GameObject bulletPrefab;

    public Vector3 velocity;

    const float THRUST_MIN_MOUSE_DIST = 0.1f;
    const float THRUST_MAX_MOUSE_DIST = 3.0f;
    const float THRUST = 0.005f;
    const float DRAG = 0.9f;
    int shotsQueued;

    ShipModule[] modules;
    Button thrustButton;
    float_field thrust;

    public void Start()
    {
        thrustButton = new Button_Mouse(0);
        thrust = new float_field(5.0f);

        ShipModule_Projectile basicAttack = new ShipModule_Projectile(bulletPrefab, 0.2f, 3.0f, 10, 5.0f, 1, 5.0f, false, new Button_Mouse(1), this);
        ShipModule_Beam beamAttack = new ShipModule_Beam(qBeam, 1, new Button_Key(KeyCode.Q), this);
        ShipModule_TimedSwitch frenzy = new ShipModule_TimedSwitch
        (
            3.0f,
            new Button_Key(KeyCode.W),
            this,
            new List<IEffect>()
            {
                basicAttack.damage + 10,
                basicAttack.bulletsPerShot + 4,
                basicAttack.firingRate + 10,
                basicAttack.automatic.Setter(true),
                thrust + 5,
            }
        );

        modules = new ShipModule[] { basicAttack, beamAttack, frenzy };

        foreach(ShipModule module in modules)
        {
            module.Start();
        }
    }

    private void Update()
    {
        Vector3 mouseTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        foreach (ShipModule module in modules)
        {
            module.Update(mouseTarget);
        }
    }

    private void FixedUpdate()
    {
        Vector3 mouseTarget = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 mouseOffset = mouseTarget - transform.position;
        mouseOffset.z = 0;

        foreach (ShipModule module in modules)
        {
            module.FixedUpdate(mouseTarget);
        }

        if (mouseOffset.sqrMagnitude > THRUST_MIN_MOUSE_DIST * THRUST_MIN_MOUSE_DIST)
        {
            Vector3 idealEuler = Quaternion.LookRotation(Vector3.forward, mouseOffset).eulerAngles;
            idealEuler.y = 0;
            idealEuler.x = 0;
            transform.rotation = Quaternion.Euler(idealEuler);// Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(idealEuler), 5.0f);
        }

        if (thrustButton.held)
        {
            if (mouseOffset.sqrMagnitude > THRUST_MAX_MOUSE_DIST * THRUST_MAX_MOUSE_DIST)
            {
                velocity += mouseOffset.normalized * THRUST_MAX_MOUSE_DIST * thrust.value * 0.001f;
            }
            else
            {
                velocity += mouseOffset * thrust.value * 0.001f;
            }
        }

        velocity *= DRAG;
        velocity.z = 0;

        transform.position += velocity;
    }
}
