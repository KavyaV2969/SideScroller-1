// Run in the connected Editor with Play Mode active:
// unity command eval_file Tests/GuardCombatLiveVerification.cs
// Uses runtime-only fixtures and virtual inputs; exit Play Mode to restore the scene.
if (!Application.isPlaying) throw new System.InvalidOperationException("Enter Play Mode first.");
var p = UnityEngine.Object.FindAnyObjectByType<Player>();
var e = UnityEngine.Object.FindAnyObjectByType<Enemy>();
var ph = p.GetComponent<PlayerHealth>(); var eh = e.GetComponent<EntityHealth>();
var pc = p.GetComponent<EntityCombat>(); var ec = e.GetComponent<EntityCombat>();
var mouse = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Mouse>("GuardVerificationMouse");
var keyboard = UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>("GuardVerificationKeyboard");
p.input.devices = new UnityEngine.InputSystem.InputDevice[] { mouse, keyboard };
var report = new System.Collections.Generic.List<string>();
string output = "Temp/GuardWork/playmode-results.txt";
System.IO.Directory.CreateDirectory("Temp/GuardWork");
Application.runInBackground = true;
var previousInputSettings=UnityEngine.Object.Instantiate(UnityEngine.InputSystem.InputSystem.settings);
UnityEngine.InputSystem.InputSystem.settings=UnityEngine.Object.Instantiate(previousInputSettings);
UnityEngine.InputSystem.InputSystem.settings.backgroundBehavior=UnityEngine.InputSystem.InputSettings.BackgroundBehavior.IgnoreFocus;
UnityEngine.InputSystem.InputSystem.settings.editorInputBehaviorInPlayMode=UnityEngine.InputSystem.InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
void Check(bool pass, string name) { report.Add((pass ? "PASS " : "FAIL ") + name); System.IO.File.WriteAllLines(output, report); if (!pass) throw new System.Exception(name); }
void Note(string message) { report.Add(message); System.IO.File.WriteAllLines(output, report); }
void Value(UnityEngine.Object obj, string path, float v) { var so = new UnityEditor.SerializedObject(obj); so.FindProperty(path).floatValue = v; so.ApplyModifiedPropertiesWithoutUndo(); }
void Flag(UnityEngine.Object obj, string path, bool v) { var so = new UnityEditor.SerializedObject(obj); so.FindProperty(path).boolValue = v; so.ApplyModifiedPropertiesWithoutUndo(); }
float Hp(EntityHealth h) => new UnityEditor.SerializedObject(h).FindProperty("health").floatValue;
void Mouse(bool block, bool attack = false) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(mouse, new UnityEngine.InputSystem.LowLevel.MouseState { buttons = (ushort)((block ? 2 : 0) | (attack ? 1 : 0)) }); }
void Keys(params UnityEngine.InputSystem.Key[] keys) { UnityEngine.InputSystem.InputSystem.QueueStateEvent(keyboard, new UnityEngine.InputSystem.LowLevel.KeyboardState(keys)); }
System.Collections.IEnumerator Frames(int n = 3) { for (int i = 0; i < n; i++) yield return null; }
void EnemyHit() { e.stateMachine.ChangeState(e.attackState); ec.PerformAttack(); }
void ResetPosition() { p.StopAllCoroutines(); Flag(p,"isKnocked",false); p.rb.linearVelocity=Vector2.zero; p.transform.position=new Vector3(0,41.3f,0); p.HandleFlip(1); e.transform.position=new Vector3(2,41.3f,0); e.HandleFlip(-1); Physics2D.SyncTransforms(); }
System.Collections.IEnumerator Scenario()
{
 e.enabled=false; e.anim.speed=0; e.rb.bodyType=RigidbodyType2D.Kinematic; e.rb.linearVelocity=Vector2.zero;
 Flag(eh,"isKnockbackImmune",true); Flag(ph,"isKnockbackImmune",false); Value(eh,"health",100000); Value(ph,"health",100000);
 foreach(var stats in new[]{p.GetComponent<EntityStats>(),e.GetComponent<EntityStats>()}) { Value(stats,"offense.critRate.baseValue",0); Value(stats,"defense.evasion.baseValue",0); }
 var ground=new GameObject("GuardVerificationGround");ground.layer=LayerMask.NameToLayer("Ground");ground.transform.position=new Vector3(0,39.5f,0);ground.AddComponent<BoxCollider2D>().size=new Vector2(100,1);
 p.SetMovementEnabled(true); p.stateMachine.ChangeState(p.idleState); Mouse(false);Keys();ResetPosition();
 yield return new WaitForSeconds(0.4f);
 Check(p.groundDetected,"fixture is grounded");
 Mouse(true);yield return new WaitForSeconds(0.1f);
 Check(p.IsBlocking,"Right Click enters held Block: state="+p.stateMachine.currentState.GetType().Name+" held="+p.BlockHeld+" device="+mouse.rightButton.isPressed);
 Check(p.GetComponentInChildren<SpriteRenderer>().sprite.name=="PlayerGraphics_22","Block holds sprite 22");
 var duplicate=p.gameObject.AddComponent<CapsuleCollider2D>();duplicate.size=p.GetComponent<CapsuleCollider2D>().size;
 float before=Hp(ph); EnemyHit();float blockedDamage=before-Hp(ph);
 Check(blockedDamage>0 && p.RiposteReady,"blocked enemy hit reduces HP and stores one riposte");
 Check(p.rb.linearVelocity.y<0.1f,"blocked hit does not launch player out of grounded guard");
 ec.PerformAttack();yield return Frames();ec.PerformAttack();
 Check(Mathf.Approximately(Hp(ph),before-blockedDamage),"one swing cannot damage twice across duplicate colliders/events/frames");
 UnityEngine.Object.Destroy(duplicate);
 float enemyBefore=Hp(eh);Mouse(true,true);yield return Frames();
 Check(p.stateMachine.currentState==p.counterAttackState && !p.RiposteReady,"Attack consumes stored riposte while Block held");
 Check(p.counterAttackState.DamageMultiplier==p.riposteDamageMultiplier,"riposte selects configured multiplier");
 Mouse(true);yield return new WaitForSeconds(0.55f);
 float riposteDamage=enemyBefore-Hp(eh);
 Check(riposteDamage>0,"riposte animation event deals damage");
 Check(p.IsBlocking && !p.RiposteReady && p.counterAttackState.DamageMultiplier==1f,"counter completes and resets to empty Block");
 EnemyHit();Check(p.RiposteReady,"new enemy swing can grant next riposte");
 Mouse(false);yield return Frames();Check(!p.RiposteReady && !p.IsBlocking,"releasing Right Click clears riposte and block immediately");
 Flag(ph,"isKnockbackImmune",true);before=Hp(ph);EnemyHit();float normalIncoming=before-Hp(ph);
 Check(Mathf.Abs(blockedDamage-normalIncoming*(1-p.blockDamageReduction))<0.01f,"75% mitigation applies to physical plus elemental damage");
 Note("Incoming normal="+normalIncoming+" blocked="+blockedDamage);
 // A normal attack after a counter must use base damage and real animation events.
 ResetPosition();p.stateMachine.ChangeState(p.idleState);yield return new WaitForSeconds(0.2f);
 enemyBefore=Hp(eh);Mouse(false,true);yield return Frames();Mouse(false);yield return new WaitForSeconds(1f);
 float normalOutgoing=enemyBefore-Hp(eh);
 Check(normalOutgoing>0,"normal attack input and animation event work after riposte");
 Check(Mathf.Abs(riposteDamage-normalOutgoing*p.riposteDamageMultiplier)<0.01f,"special multiplier does not leak into normal attack");
 // Successful parry is checked at the existing enemy overlap event.
 ResetPosition();p.stateMachine.ChangeState(p.idleState);yield return new WaitForSeconds(0.2f);Mouse(true);yield return Frames();
 Mouse(true,true);yield return Frames(1);Mouse(true);
 Check(p.stateMachine.currentState==p.parryState,"Block + Attack without riposte enters Parry");
 Check(!p.parryState.IsActive,"parry startup is not active");
 yield return new WaitForSeconds(p.parryStartup+0.01f);
 Check(p.parryState.IsActive,"parry has a timed active window");
 before=Hp(ph);enemyBefore=Hp(eh);EnemyHit();
 Check(p.stateMachine.currentState==p.counterAttackState,"region overlap automatically starts parry counter");
 Check(Hp(ph)==before,"successful parry causes exactly zero incoming damage");
 Check(p.counterAttackState.DamageMultiplier==p.parryCounterDamageMultiplier,"parry counter selects higher multiplier");
 ec.PerformAttack();ec.PerformAttack();yield return new WaitForSeconds(0.55f);ec.PerformAttack();
 float parryDamage=enemyBefore-Hp(eh);
 Check(Hp(ph)==before,"parried attack cannot damage later after active window ends");
 Check(Mathf.Abs(parryDamage-normalOutgoing*p.parryCounterDamageMultiplier)<0.01f,"one successful parry produces exactly one 3x animation-driven counter");
 Check(p.IsBlocking && !p.RiposteReady,"successful parry leaves no stored riposte");
 Note("Outgoing normal="+normalOutgoing+" riposte="+riposteDamage+" parry="+parryDamage);
 // Exercise the enemy's real Animator attack event as well as repeated overlap calls.
 Mouse(true,true);yield return Frames(1);Mouse(true);yield return new WaitForSeconds(p.parryStartup+0.01f);
 var enemyClip=e.anim.runtimeAnimatorController.animationClips.First(c=>c.name=="Enemy_KnightAttack");
 float hitTime=UnityEditor.AnimationUtility.GetAnimationEvents(enemyClip).First(x=>x.functionName=="attackTrigger").time;
 before=Hp(ph);enemyBefore=Hp(eh);e.stateMachine.ChangeState(e.attackState);e.anim.speed=1;
 e.anim.Play("Enemy_KnightAttack",0,0);e.anim.Update(0);e.anim.Update(hitTime+0.001f);e.anim.speed=0;
 Check(p.stateMachine.currentState==p.counterAttackState && Hp(ph)==before,"real enemy Animator attack event is parried before damage");
 yield return new WaitForSeconds(0.55f);Check(Mathf.Abs((enemyBefore-Hp(eh))-normalOutgoing*p.parryCounterDamageMultiplier)<0.02f,"real enemy event automatically produces exactly one counter");
 // Miss, including lockout against dash, jump and re-block.
 Mouse(true,true);yield return Frames(1);Mouse(true);yield return new WaitForSeconds(p.parryStartup+p.parryActiveDuration+0.02f);
 Check(p.stateMachine.currentState==p.parryRecoveryState && !p.IsBlocking && !p.parryState.IsActive,"missed parry ends active window and block protection");
 before=Hp(ph);EnemyHit();Check(Mathf.Abs((before-Hp(ph))-normalIncoming)<0.01f && !p.RiposteReady,"missed parry recovery takes full damage without riposte");
 Keys(UnityEngine.InputSystem.Key.D,UnityEngine.InputSystem.Key.Space,UnityEngine.InputSystem.Key.LeftShift);Mouse(true,true);yield return Frames();
 Check(p.stateMachine.currentState==p.parryRecoveryState,"movement/jump/dash/Attack cannot cancel vulnerable recovery");
 Keys();Mouse(true);yield return new WaitForSeconds(p.parryRecoveryDuration+0.1f);
 Check(p.IsBlocking,"holding Right Click returns to Block after recovery");
 // Region mismatch during active timing is not a parry.
 var savedOffset=p.parryRegionOffset;p.parryRegionOffset=new Vector2(100,0);
 Mouse(true,true);yield return Frames(1);Mouse(true);yield return new WaitForSeconds(p.parryStartup+0.01f);
 before=Hp(ph);EnemyHit();Check(Hp(ph)<before && p.stateMachine.currentState!=p.counterAttackState,"active timing without region overlap does not parry");
 p.parryRegionOffset=savedOffset;yield return new WaitForSeconds(0.6f);
 EnemyHit();Check(p.RiposteReady,"riposte available before disable reset");
 p.SetMovementEnabled(false);Check(!p.RiposteReady && !p.input.Player.Block.enabled,"movement/dialogue disable clears guard and disables Block action");
 p.SetMovementEnabled(true);Mouse(false);yield return Frames();
 // Component disable must immediately discard special damage, not defer it until re-enable.
 Mouse(true);yield return Frames();p.StartCounterAttack(true);p.enabled=false;
 Check(!p.RiposteReady && p.counterAttackState.DamageMultiplier==1f,"component disable immediately resets special attack state");
 p.enabled=true;Mouse(false);yield return Frames();Check(p.stateMachine.currentState!=p.counterAttackState,"re-enable does not resume cancelled counter");
 // Movement, jump, aerial guard rejection and dash.
 ResetPosition();p.stateMachine.ChangeState(p.idleState);yield return new WaitForSeconds(0.2f);
 Keys(UnityEngine.InputSystem.Key.D);yield return Frames();Check(p.stateMachine.currentState==p.moveState && p.rb.linearVelocity.x>0,"existing ground movement works");
 Keys();yield return Frames();Keys(UnityEngine.InputSystem.Key.Space);yield return Frames(1);Keys();yield return new WaitForSeconds(0.12f);
 Check(!p.groundDetected && p.rb.linearVelocity.y>0,"existing jump works: state="+p.stateMachine.currentState.GetType().Name+" grounded="+p.groundDetected+" velocity="+p.rb.linearVelocity);
 Mouse(true);yield return Frames();Check(!p.IsBlocking && p.stateMachine.currentState!=p.blockState,"holding Block in air cannot guard");
 Mouse(false);Keys(UnityEngine.InputSystem.Key.LeftShift);yield return Frames();Check(p.stateMachine.currentState==p.dashState,"existing aerial dash works");
 Keys();yield return new WaitForSeconds(0.3f);
 Check(p.stateMachine.currentState!=p.dashState && p.rb.gravityScale>0,"dash exits and restores gravity");
 // Jump attack uses its original landing attack event.
 ResetPosition();p.stateMachine.ChangeState(p.idleState);yield return new WaitForSeconds(0.2f);Keys(UnityEngine.InputSystem.Key.Space);yield return new WaitForSeconds(0.12f);Keys();
 Mouse(false,true);yield return Frames();Mouse(false);Check(p.stateMachine.currentState==p.jumpAttackState,"existing jump attack input works");
 yield return new WaitForSeconds(1.2f);Check(p.stateMachine.currentState!=p.jumpAttackState,"jump attack landing/completion events return to locomotion");
 // Full three-hit combo through Attack presses, not direct state construction.
 ResetPosition();p.stateMachine.ChangeState(p.idleState);yield return new WaitForSeconds(1.1f);
 Mouse(false,true);yield return Frames();Mouse(false);yield return new WaitForSeconds(0.1f);Mouse(false,true);yield return Frames();Mouse(false);
 yield return new WaitForSeconds(0.68f);Check(p.anim.GetInteger("basicAttackComboIndex")==2,"normal combo advances to second attack");
 Mouse(false,true);yield return Frames();Mouse(false);yield return new WaitForSeconds(0.55f);Check(p.anim.GetInteger("basicAttackComboIndex")==3,"normal combo advances to third attack");
 yield return new WaitForSeconds(0.8f);Check(p.stateMachine.currentState!=p.attackState,"normal combo completes");
 // Death interrupts guard without allowing a delayed counter or state restart.
 ResetPosition();p.stateMachine.ChangeState(p.idleState);yield return new WaitForSeconds(0.2f);Mouse(true);yield return Frames();EnemyHit();
 p.StartCounterAttack(true);Check(p.counterAttackState.DamageMultiplier>1f,"special multiplier armed before death interruption");
 Value(ph,"health",1);ph.TakeDamage(100000,0,ElementType.None,e.transform);yield return Frames();
 Check(p.stateMachine.currentState==p.deadState && !p.RiposteReady && !p.input.Player.Block.enabled,"death clears guard and disables inputs");
 Check(!p.rb.simulated && p.counterAttackState.DamageMultiplier==1,"death preserves existing physics shutdown and resets special damage");
 Note("All gameplay assertions passed.");
}
System.Collections.IEnumerator Run()
{
 var test=Scenario();
 while(true)
 {
  bool next;object item=null;
  try { next=test.MoveNext();if(next)item=test.Current; }
  catch(System.Exception ex) { Note("EXCEPTION "+ex);break; }
  if(!next)break;
  yield return item;
 }
 UnityEngine.InputSystem.InputSystem.settings=previousInputSettings;
 p.input.devices=null;UnityEngine.InputSystem.InputSystem.RemoveDevice(mouse);UnityEngine.InputSystem.InputSystem.RemoveDevice(keyboard);
 if(!report.Any(line=>line.StartsWith("FAIL")||line.StartsWith("EXCEPTION"))) Note("COMPLETE: live Play Mode verification passed, virtual devices and input settings restored.");
}
// Host on the enemy so resetting player coroutines and killing the player cannot stop the verifier.
e.StartCoroutine(Run());return "Started live verification; results: "+output;
