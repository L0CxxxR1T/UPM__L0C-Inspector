# L0C\_Basics: Inspector

属性ベースのインスペクタ拡張。Unity 6.3以降 / UI Toolkit。

## 何が違うか

`[CustomPropertyDrawer(typeof(FooAttribute))]`方式だと、Unityは1フィールドに1つの`PropertyDrawer`しか
適用しないため`[Required]`と`[ReadOnly]`を併記すると片方が気づかないうちに無効になる。

このパッケージは**属性の解釈を中央Editorに集約**し、`PropertyField`に対して属性を順に適用する。
そのため1フィールドに何個属性を付けても全部効く。

```
型に対する描画   → PropertyDrawer      （独自の値型など。Dictionary は Unity 標準の Drawer を使う）
属性による装飾   → 中央 Editor が解釈   （Required / ReadOnly / 条件 / 検証 / グループ）
```

## 使い方

属性を付けるだけ。フォールバックEditorが`MonoBehaviour`と`ScriptableObject`に対して登録されている。

```csharp
using UnityEngine;
using XXXL0C.Inspector;

public sealed class Player : MonoBehaviour
{
    [Required]
    [SerializeField] private Rigidbody _body;

    [Required]
    [ReadOnly]
    [SerializeField] private Transform _spawnPoint;
}
```

扱う属性が1つも付いていないクラスは標準インスペクタをそのまま出すので、
他のアセットのコンポーネントの見た目は変わらない。
例外はDictionaryフィールドを持つクラスで、属性が無くても追加欄と重複キーの検証が付く
（「Dictionary（Unity 6.6以降）」を参照）。

### 個別CustomEditorと併用する

基底クラスの継承は不要。静的ビルダーを呼んで、返ってきた`VisualElement`に足す。

```csharp
[CustomEditor(typeof(Player))]
public sealed class PlayerEditor : UnityEditor.Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        VisualElement root = InspectorBuilder.Build(this);
        root.Add(new Button(Reset) { text = "初期化" });
        return root;
    }
}
```

`InspectorBuilder.Build()`を呼び忘れると属性機能が出なくなるだけで、気づかないうちにおかしくなることはない。

## 属性

### `[Required]`

未設定を検出する。「未設定」の定義は以下。

- `Object`参照がnull
- `[SerializeReference]`がnull
- 配列 / `List`の要素がnull

空リストと空文字は**対象外**（正当なケースが多く、含めると誤検知で警告が無視されるようになる）。
`int` / `bool` / enumなど「未設定」概念の無い型に付けた場合は、
`HelpBox`で「この型では機能しない」と明示する。

### `[ReadOnly]`

インスペクタでの編集を禁止し、表示のみにする。
`SetEnabled(false)`ではなく入力を止める方式なので、リストや折りたたみの開閉とスクロールはできる。

### `[ShowIf]` / `[HideIf]`

条件を満たすときだけ表示する。条件対象は**フィールド限定**で、指定は`nameof`を使う。

```csharp
[SerializeField] private bool _useCustomTarget;

[ShowIf(nameof(_useCustomTarget))]
[Required]
[SerializeField] private Transform _customTarget;

[ShowIf(nameof(_mode), Mode.Custom)]
[SerializeField] private float _weight;
```

- `[ShowIf]`を複数付けるとAND。`[HideIf]`はどれか1つ満たせば隠れる
- **非表示のフィールドは`[Required]`を評価しない**（設定できないものを未設定として警告しない）
- 条件が評価できないとき（`nameof`のタイポ、型の食い違い）は**隠さず`HelpBox`で警告する**。
  隠すと設定もできず原因も見えなくなるため
- メソッドやプロパティを条件にできないのは意図的。将来のビルド前バリデータが条件を評価する際に
  アセット上でユーザーコードを実行することになり、副作用があると思わぬ問題が起きる

### `[EnableIf]` / `[DisableIf]`

フィールドを表示したまま編集可否を条件で切り替える。条件の指定方法は`[ShowIf]`と同じで、対象はフィールド、指定は`nameof`。
`[EnableIf]`は条件が成立したときだけ編集でき、`[DisableIf]`は成立したとき表示のみになる。
編集できないフィールドは検証対象から外れる。

### `[NotEmpty]` / `[MinValue]` / `[MaxValue]`

- `[NotEmpty]`は文字列・配列・Listが空のときWarningを出す。メッセージは省略できる
- `[MinValue]` / `[MaxValue]`は数値の下限・上限を指定する。編集値は範囲内に丸め、範囲外の値は検証でも検出する

```csharp
[NotEmpty("名前を入力してください")]
[SerializeField] private string _name;

[MinValue(1)]
[MaxValue(20)]
[SerializeField] private int _count = 4;
```

### Object参照の制約

- `[AssetsOnly]`はProject内のアセットだけを許可する
- `[SceneObjectsOnly]`はシーン上のオブジェクトだけを許可する
- `[ChildGameObjectsOnly]`はこのコンポーネントの子孫を許可する。`ChildGameObjectsOnly(false)`で自分自身を除外できる

違反は検証層がErrorとして報告する。各属性はGameObject / Componentの参照に使う。

### `[HideInPlayMode]` / `[HideInEditMode]` / `[DisableInEditMode]`

Play ModeまたはEdit Modeに応じてフィールドを隠す、または編集不可にする。インスペクタを開いたままモードを切り替えても追従する。

### `[HorizontalGroup]` / `[TabGroup]`

フィールドを横並び、またはタブページにまとめる。`HorizontalGroup`のパスはグループ名、`TabGroup`は入れ物とページ名を指定する。
タブページを含むパスは「外側のグループ / 入れ物 / ページ」の順にする。

```csharp
[HorizontalGroup("位置")]
[SerializeField] private float _x;

[TabGroup("詳細", "基本")]
[SerializeField] private string _description;
```

### `[BoxGroup]` / `[FoldoutGroup]`

見た目のグループにまとめる。`"/"`で区切ると入れ子になる。

```csharp
private const string GROUP_BATTLE = "戦闘";
private const string GROUP_DAMAGE = "戦闘/ダメージ";

[FoldoutGroup(GROUP_BATTLE)]
[SerializeField] private Collider _hitBox;

[FoldoutGroup(GROUP_DAMAGE)]
[SerializeField] private int _damage;
```

- グループは「そのグループに属する**最初のフィールドが現れた位置**」に出る。
  順序引数は無く、並びはフィールドの宣言順で決まる
- 配下に検証エラーがあるとヘッダに件数バッジが出る。入れ子なら親にも伝播する。
  **エラーが出ても折りたたみを自動で開くことはしない**
- `[ShowIf]`で子が全部消えたら**グループごと消える**。条件はフィールド側1箇所にだけ書く
  （グループ属性は条件を持たない）
- グループ名はマジックストリングを散らさないよう`const`にまとめる
- グループは見た目の話でしかない。シリアライズ構造やクラス設計をグループ都合で変えないこと

### `[Label]` / `[Prefix]` / `[Suffix]`

```csharp
[Label("表示名を変更")]
[SerializeField] private int _renamed;

[Prefix("x")]
[Suffix("m/s")]
[SerializeField] private float _speed;
```

`[Prefix]`と`[Suffix]`は同時に付けても両方効く（旧`PropertyDrawer`方式では1フィールド1 Drawerの
制約で片方しか適用されなかった）。

### `[ReadOnlyInPlayMode]`

Play Mode中だけ編集を禁止する。インスペクタを開いたままPlay Modeに出入りしても正しく追従する。
`[ReadOnly]`と併記しても打ち消し合わない。

### `[OnValueChanged]`

値が変わるたびに、同じインスタンス上の引数なしメソッドを呼ぶ。メソッド名のタイポは検証層が
Warningで報告するので、一括チェックウィンドウでもプロジェクト横断で拾える。
複数選択中に値を変えると、選択中の全オブジェクトで呼ばれる。

```csharp
[OnValueChanged(nameof(OnCountChanged))]
[SerializeField] private int _count;

private void OnCountChanged() { /* ... */ }
```

### `[SteppedRange]` / `[SteppedMinMaxSlider]` / `[SceneName]` / `[TypeFilter]`

`PropertyField`そのものを専用コントロールに差し替える属性。対応していない型に付けた場合は
標準の`PropertyField`にフォールバックし、`HelpBox`で理由を通知する（検証メッセージとは別の
永続領域に出るので、値を変えても消えない）。

```csharp
[SteppedRange(0f, 10f, 0.5f)]
[SerializeField] private float _stepped;

[SteppedMinMaxSlider(0f, 100f, 5f)]
[SerializeField] private FloatRange _range; // _min / _max（または min / max）を持つ型。int 同士も可

[SceneName]
[SerializeField] private string _targetScene; // ビルド対象のシーン名から選ぶ

[TypeFilter]
[SerializeReference] private IShape _shape; // 実装クラスをドロップダウンで選ぶ
```

- `[Label]` / `[OnValueChanged]` / `[Tooltip]` / `[Header]` / `[Space]`は差し替えた本体にも効く
- `FloatRange` / `IntRange`はパッケージ側で用意している。`[SteppedMinMaxSlider]`にそのまま使える
- `[SceneName]`の候補はアクティブなBuild Profileのシーンリスト（有効なものだけ）。
  一覧に無い名前が入っていると検証層がErrorを出す（空文字は対象外）
- `[TypeFilter]`の候補は、フィールドの型（または`[TypeFilter(typeof(基底型))]`）を継承した
  `[Serializable]`で引数なしコンストラクタを持つ具象クラス。`[SerializeReference]`の配列・`List`にも使える
  （`SerializableType`は型名を保存し、配列・`List`でも使える。詳しくは後述の「`SerializableType`」）

### `[EnumToggleButtons]` / `[PreviewField]` / `[ValueDropdown]`

- `[EnumToggleButtons]`はenumを横並びのボタンにする。`[Flags]` enumは複数選択できる
- `[PreviewField]`はObject参照欄の右にプレビューを表示する。サイズは属性の引数で指定する
- `[ValueDropdown(nameof(候補フィールド))]`は`static readonly`の配列・List、または`ValueDropdownList<T>`から値を選ぶ。メソッドやプロパティは候補元にできない

### `[InlineEditor]`

Object参照フィールドの下に、参照先のインスペクタをそのまま埋め込む。
AとBが互いを参照している場合は1周で止め、入れ子は3段までにする（超えた分は案内だけ出す）。

### `[Button]` / `[ButtonGroup]`

メソッドを呼び出すボタンを表示する。フィールドではなくメソッドが対象なので、他の属性とは別軸で処理される。
引数欄は`int` / `long` / `float` / `double` / `string` / `bool` / enum / `Vector2`〜`Vector4` / `Vector2Int` / `Vector3Int` / `Color` / Object派生型に対応する。
既定値は引数の既定値を使い、戻り値はボタンの下に表示する。未対応型、`ref`、`out`引数はHelpBoxで通知する。

```csharp
[Button]
private void DoSomething() { /* ... */ }

[Button("加算", group: "操作")]
[ButtonGroup("カウント")]
private int Add(int amount = 1) => _count += amount;

[Button("実行時だけ有効", ButtonMode.PlayModeOnly)]
private void RuntimeOnlyAction() { /* ... */ }
```

- staticメソッドは1回だけ呼ぶ。instanceメソッドは選択中の各オブジェクトで呼び、1回のUndoにまとめる
- グループ未指定なら**フッター**（全フィールドの後ろ）に出る。指定すれば`[BoxGroup]` / `[FoldoutGroup]`
  と同じパス文字列で、既存グループの中に入れられる（無ければ自動で`[FoldoutGroup]`として作られる）
- `[ButtonGroup("名前")]`を併記すると、同じ置き場所にあるボタンを横並びにする
- `ButtonMode.EditorOnly` / `PlayModeOnly`を指定すると、対象外のモードでは無効化される
  （隠すのではなく`SetEnabled(false)`。理由はツールチップに出る）
- ボタンだけのグループ（フィールドが1つも無い）でも、空グループとして隠れることはない

## 値の型

### `Optional<T>`

「値が無い」状態を持てる値。インスペクタでは値の欄の右にチェックボックスが付き、外すと値の欄が表示のみになる。

```csharp
[SerializeField] private Optional<int> _overrideCount;

if (_overrideCount.TryGetValue(out int count)) { /* ... */ }
```

### `EnumIndexedList<TEnum, TValue>`（Unity 6.5以前向け）

enumの各メンバーに値を1つずつ持つ表。インスペクタではenumのメンバー名を見出しにして並ぶ。
**Unity 6.6以降は標準でシリアライズされる`Dictionary<TEnum, TValue>`を使う方がよい。**

- 値はenumの宣言順の位置で保存する。メンバーを途中に挿入・並べ替えると値がずれる（末尾への追加は安全）
- 値が飛び飛びのenumにも使える。同じ値の別名は同じ要素を指す

### `SerializableType`

インスペクタで選んだ型を保存する。保存するのは型の完全名だけで、**実行時に`Type`へ戻すときは
`SerializableTypeRegistry`に明示的に登録した型からしか引かない**。名前からリフレクションで型を解決すると、
IL2CPPのコードストリッピングで型が消えたときにビルドでだけ気づかないうちにおかしくなるため。

```csharp
[Required]
[TypeFilter(typeof(IEnemy))]
[SerializeField] private SerializableType _enemyType;

// 起動時（Composition Root など）に、引く可能性のある型をすべて登録する
SerializableTypeRegistry.Register<SlimeEnemy>();
SerializableTypeRegistry.Register<GoblinEnemy>();

Type enemyType = _enemyType.Resolve(); // 未設定・未登録なら例外
```

- `[TypeFilter(typeof(基底型))]`で候補を絞ったドロップダウンになる（基底型の指定が必須）。配列・`List`の要素にも効く
- 保存している型名が候補に無い（クラスの改名・削除、基底型の変更）と、検証層がErrorを出す
- `[Required]`を付けると、型が未選択のときにErrorを出す
- Editorでも登録表しか見ない。Editorでは動くのにビルドでは動かない、という食い違いを作らないため

## Dictionary（Unity 6.6以降）

Unity 6.6の標準Dictionaryシリアライズ（`[SerializeField] Dictionary<TKey, TValue>`）に対応する。
属性を付けなくても、Dictionaryフィールドを持つクラスはこのパッケージのインスペクタで表示される。
6.6未満ではUnityがDictionaryをシリアライズしないので、以下の処理はすべて無効になる。

```csharp
[Required]
[SerializeField] private Dictionary<string, GameObject> _prefabs = new Dictionary<string, GameObject>();
```

- 表示はUnity標準のDrawer（2カラム表示、`[DictionaryDisplay]`）に任せる
- **追加は自前の追加欄に一本化する。** キーを入力した時点で重複とnullを弾くので、重複したまま要素は増えない。
  標準の「+」は選択中の要素を複製し、押した時点で重複キーができるため隠す。複製・貼り付けのコマンドも止める
  - 入力欄があるキー型: `string` / 整数 / 浮動小数 / `bool` / enum / `UnityEngine.Object` / `Vector2(Int)` / `Vector3(Int)` / `Color`
  - それ以外（独自の構造体など）は既定値のキーで追加し、表の中で書き換える。既定値のキーが既にあれば追加できない
- **重複キーは属性の有無に関わらずError。** 表の中でキーを書き換えれば重複は作れるため、検証層でも拾う。
  実行時は最初の要素だけが使われ、Playerでは知らないうちに捨てられる
- `[Required]`を付けると、キーのnull（Object参照キーの未設定・参照切れ）と値のnullを検出する。
  キーにも値にも「未設定」の概念が無い型（`Dictionary<string, int>`など）ではHelpBoxで通知する
- 値のクラスの中の`[Required]` / `[ShowIf]`も検証される。装飾属性はListと同じく反映されない（Warningで通知）
- 追加欄が付くのは、このパッケージが行を組むフィールド（トップレベルと自前展開したネストクラス）だけ。
  Listの要素の中やDictionaryの値の中のDictionary、カスタムDrawerを持つ型の中は標準の動作のまま
  （重複キーの検証は効く）

自前の`SerializableDictionary`は作らない。Unity 6.6の標準Dictionaryシリアライズを使う。

## 拡張する

属性を足すときは「属性クラス + 実装クラス」の2つだけ書けばよい。
`TypeCache`で自動収集されるので登録は不要（引数なしコンストラクタが必要）。

| 拡張点 | 走るタイミング | 責務 |
|---|---|---|
| `IPropertyDecorator` | 構築時1回 | 1フィールドの見た目を加工する（複数個を重ねられる） |
| `IFieldFactory` | 構築時1回 | 1フィールドの本体を作る（1つしか適用できない） |
| `IVisibilityRule` | 更新時（検証より前） | 表示 / 非表示 |
| `IEnabledRule` | 更新時（検証より前） | 編集可否 |
| `IValidationRule` | 更新時 | メッセージ |
| `IGroupContainerFactory` | 構築時1回 | グループコンテナの種類 |
| `IGroupPageFactory` | 構築時1回 | タブなどページを持つグループ |

`[Button]`はメソッド対象という別軸の性質上、上記の拡張点システムには乗っていない
（`ButtonSectionBuilder`が直接処理する）。

```csharp
public sealed class TooltipDecorator : IPropertyDecorator
{
    public Type AttributeType => typeof(MyTooltipAttribute);
    public int Order => 0;

    public void Decorate(DecorationContext context)
        => context.Field.tooltip = ((MyTooltipAttribute)context.Attribute).Text;
}
```

検証は装飾の一種ではなく、ツリー構築後に何度も走り直す独立した層。
値が変わると`TrackSerializedObjectValue`経由で再実行される。

兄弟プロパティを参照したいときは`PropertyPathUtility.FindSibling()`を使う。
ルートから名前で探す実装は配列要素内・ネストクラス内で正しく動かない。

## ネストしたクラスの扱い

ネストした`[Serializable]`クラスは自前で展開するので、中のフィールドにも装飾属性が効く。

ただし**カスタム`PropertyDrawer`を持つ型は展開しない**（そのDrawerの描画を奪わないため）。
展開されなかったネストの中に装飾属性があった場合は、`HelpBox`で未反映であることを通知する。
`[SerializeReference]`も現時点では展開対象外。

検証（`IValidationRule`）は展開の有無に関わらず、ネスト・配列要素まで再帰する。

## 検証の実行タイミング（3層）

`[Required]`は「インスペクタで赤くする」だけでは保証にならない（開いていないシーン・プレハブは誰も見ない）。
3層で運用する。

1. **インスペクタ**: 開いたときに即時表示（ここまでは上記のとおり）
2. **一括チェックウィンドウ**: `Window > XXXL0C > インスペクタ検証`。
   プロジェクト全体（全プレハブ・全ScriptableObject・全シーン）を走査する。
   動的ロード（Addressables / `Resources`）で読まれるアセットの取りこぼしをここで拾う。
   シーン走査は重いので「シーンも走査する」トグルで切れる（既定オン）
3. **ビルド前フック**: `IPreprocessBuildWithReport`。
   **ビルド対象シーンとその依存アセットに限定**して、Errorが1件でもあればビルドを止める。
   全アセット走査にしないのは、未使用の作りかけプレハブで誤検知が出て
   警告全体が無視されるようになるのを防ぐため

3層とも同じ判定ロジック（`VisibilityEvaluator` / `ValidationWalker` / `ObjectValidator`）を通るので、
「インスペクタでは何も出ないのにビルドが止まる」という食い違いは起きない。

ネストしたプレハブ・Variant・シーン上のプレハブインスタンスは、元のプレハブと同じ結果を報告しない
（元のプレハブ側で1回だけ出る）。オーバーライドで結果が変わったものだけがインスタンス側に出る。

ビルド対象のシーンは`EditorBuildSettings.scenes`から取る。アクティブなBuild Profileがシーンリストを
上書きしていればそちらが使われる。アクティブでないプロファイルを指定したビルドや、スクリプトから
シーンを直接渡したビルドは判別できない。

`Project Settings > XXXL0C > インスペクタ検証`で以下を切り替えられる。

- **ビルド時にエラーで止める**（既定オン）。締切直前にオフにできるよう、あえてスイッチとして残してある
- **ビルド時にシーンも検証する**（既定オン）

設定は`ProjectSettings/XXXL0CInspectorValidation.asset`に保存されるので`Assets/`を汚さず、
バージョン管理にも自然に乗る。

## 現在の制限

- `[SerializeReference]`の中身は展開しない（具象型が実行時に変わるとツリーの作り直しが必要になるため）。
  検証も対象外
- 配列 / `List`の要素は1行ずつ展開せず、`PropertyField`のリスト描画に任せる。
  要素のクラスの中の装飾属性は反映されない（Warningで通知する）
- 複数選択編集中は検証しない（混在値で誤検知が出るため）。Dictionaryの追加欄も出さない
  （Unity標準のDictionary表示が複数選択編集に対応していないため）
- `[ShowIf]`の値比較はbool / enum / 整数 / float / string / Object参照の有無まで。
  フラグenumの部分一致は未対応
- `[Header]` / `[Space]`以外のDecoratorDrawerは、本体を差し替えたフィールドと自前展開したネストクラスでは出ない

## テスト

UIに依存しない処理（`ConditionEvaluator` / `PropertyPathUtility` / パスの分解 / ステップ丸めなど）は
`Tests/EditMode`にEditModeテストがある。Test RunnerのEditModeタブから実行できる。
