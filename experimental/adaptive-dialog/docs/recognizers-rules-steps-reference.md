# Adaptive dialog: Recognizers, rules, steps and inputs

This document describes the constituent parts of Adaptive dialog. 

- [Recognizers](#Recognizers)
- [Rules](#Rules)
- [Steps](#Steps)
- [Inputs](#Inputs)

## Recognizers
_Recognizers_ help understand and extract meaningful pieces of information from user's input. All recognizers emit events - of specific interest is the 'recognizedIntent' event that fires when the recognizer picks up an intent (or extracts entities) from given user utterance.

Adaptive Dialogs support the following recognizers - 
- [RegEx recognizer](#RegEx-recognizer)
- [LUIS recognizer](#LUIS-recognizer)
- [Multi-language recogizer](#Multi-language-recognizer)

### RegEx recognizer
RegEx recognizer enables you to extract intent and entities based on regular expression patterns. 

``` C#
var rootDialog = new AdaptiveDialog("rootDialog")
{
    Recognizer = new RegexRecognizer()
    {
        Intents = new Dictionary<string, string>()
        {
            { "AddIntent", "(?i)(?:add|create) .*(?:to-do|todo|task)(?: )?(?:named (?<title>.*))?"},
            {  "DeleteIntent", "(?i)(?:delete|remove|clear) .*(?:to-do|todo|task)(?: )?(?:named (?<title>.*))?" },
            { "ShowIntent", "(?i)(?:show|see|view) .*(?:to-do|todo|task)|(?i)(?:show|see|view)" },
            { "ClearIntent", "(?i)(?:delete|remove|clear) (?:all|every) (?:to-dos|todos|tasks)" },
            { "HelpIntent", "(?i)help" },
            { "CancelIntent", "(?i)cancel|never mind" }
        }
    }
}
```

**Note:** RegEx recognizer will emit a 'None' intent when the input utterance does not match any defined intent.

### LUIS recognizer
[LUIS.ai][1] is a machine learning-based service to build natural language into apps, bots, and IoT devices. LUIS recognizer enables you to extract intents and entities based on a LUIS application. 

``` C#
var rootDialog = new AdaptiveDialog("rootDialog")
{
    Recognizer = new LuisRecognizer(new LuisApplication("<LUIS-APP-ID>", "<AUTHORING-SUBSCRIPTION-KEY>", "<ENDPOINT-URI>"))
}
```

### Multi-language recognizer
When building sophisticated multi-lingual bot, you will typically have one recognizer tied to a specific language x locale. Multi-language recognizer enables you to easily specify the recognizer to use based on the [locale][2] property on the incoming activity from user. 

``` C#
var rootDialog = new AdaptiveDialog("rootDialog")
{
    Recognizer = new MultiLanguageRecognizer()
    {
        Recognizers = new Dictionary<String, IRecognizer>()
        {
            { "en", new RegexRecognizer()
                    {
                        Intents = new Dictionary<string, string>()
                        {
                            { "AddIntent", "(?i)(?:add|create) .*(?:to-do|todo|task)(?: )?(?:named (?<title>.*))?"},
                            { "DeleteIntent", "(?i)(?:delete|remove|clear) .*(?:to-do|todo|task)(?: )?(?:named (?<title>.*))?" },
                            { "ShowIntent", "(?i)(?:show|see|view) .*(?:to-do|todo|task)|(?i)(?:show|see|view)" },
                            { "ClearIntent", "(?i)(?:delete|remove|clear) (?:all|every) (?:to-dos|todos|tasks)" },
                            { "HelpIntent", "(?i)help" },
                            { "CancelIntent", "(?i)cancel|never mind" }
                        }
                    } 
            },
            { "fr", new LuisRecognizer(new LuisApplication("<LUIS-APP-ID>", "<AUTHORING-SUBSCRIPTION-KEY>", "<ENDPOINT-URI>")) }
        }
    }
}
```

## Rules
_Rules_ enable you to catch and respond to events. The broadest rule is the EventRule that allows you to catch and attach a set of steps to execute when a specific event is emitted by any sub-system. Adaptive dialogs supports couple of other specialized rules to wrap common events that your bot would handle.

Adaptive dialogs support the following Rules - 
- [Intent rule](#Intent-rule)
- [Unknown intent rule](#Unknown-intent-rule) 
- [Event rule](#Event-rule)

At the core, all rules are event handlers. Here are the set of events that can be handled via a rule.
<a name="events"></a>

| Event               | Description                                       |
|---------------------|---------------------------------------------------|
| BeginDialog         | Fired when a dialog is start                      |
| ActivityReceived    | Fired when a new activity comes in                |
| RecognizedIntent    | Fired when an intent is recognized                |
| UnknownIntent       | Fired when an intent is not handled or recognized |
| StepsStarted        | Fired when a plan is started                      |
| StepsSaved          | Fires when a plan is saved                        |
| StepsEnded          | Fires when a plan successful ends                 |
| StepsResumed        | Fires when a plan is resumed from an interruption |
| ConsultDialog       | fired when consulting                             |
| CancelDialog        | fired when dialog canceled                        |

### Intent rule
Intent rule enables you to catch and react to 'recognizedIntent' event emitted by a recognizer. All built-in recognizers emit this event when they successfully process an input utterance. 

``` C#
// Create root dialog as an Adaptive dialog.
var rootDialog = new AdaptiveDialog("rootDialog");
            
// Create an intent rule with the intent name
var bookFlightRule = new IntentRule("Book_Flight");

// Create steps when the bookFlightRule triggers
var bookFlightSteps = new List<IDialog>();
bookFlightSteps.Add(new SendActivity("I can help you book a flight"));
bookFlightRule.Steps = bookFlightSteps;

// Add the bookFlight rule to root dialog
rootDialog.AddRule(bookFlightRule);
```
**Note:** You can use the IntentRule to also trigger on 'entities' generated by a recognizer. 

### UnknownIntentRule
Use this rule to catch and respond to a case when a 'recognizedIntent' event was not caught and handled by any of the other rules. This is especially helpful to capture and handle cases where your dialog wishes to participate in consultation.

``` C# 
// Create root dialog as an Adaptive dialog.
var rootDialog = new AdaptiveDialog("rootDialog");

// Handle unknown intent.
// Note: Showing inline rule definition. 
rootDialog.AddRule(new UnknownIntentRule()
{
    Steps = new List<IDialog>()
    {
        new SendActivity("Sorry, I do not understand that. Try saying 'who are you' or 'what can you do")
    }
});
```

### Event rule
As the name suggests, event rule enables you to catch and react to [events](#events) raised by any of the sub-system or your own dialog. Remember each step is a dialog and you can use the [EmitEvent](#Emit-event) step to emit a custom event. 

``` C#
// Create root dialog as an Adaptive dialog.
var rootDialog = new AdaptiveDialog("rootDialog");
rootDialog.AddRule(new EventRule([AdaptiveEvents.ActivityReceived], new List<IDialog>()
{
    new SendActivity("Event {turn.ActivityReceived.value} received")
});
```

## Inputs 
_Inputs_ are wrappers around [prompts][2] that you can use in an adaptive dialog step to ask and collect a piece of input from user, validate and accept it into memory. Inputs include these pre-built features - 
- Accepts a property to bind to off the new V3 style memory scope in V4. 
- Performs existential check before prompting. 
- Grounds input to the specified property if the input from user matches the type of entity expected. 
- Accepts constraints - min, max, etc. 

Adaptive dialogs support the following inputs - 
- [TextInput](#TextInput)
- [ChoiceInput](#ChoiceInput)
- [ConfirmInput](#ConfirmInput)
- [NumberInput](#NumberInput)

### TextInput
Use text input when you want to verbatim accept user input as value for a specific piece of information your bot is trying to collect. E.g. user's name, subject of an email etc. 

**Note:** By default, TextInput will trigger a consultation before accepting user input. This is to allow a parent dialog to handle the specific user input in case their recognizer had a more definitive match via an intent or entity.

``` C#
// Create an adaptive dialog.
var getUserNameDialog = new AdaptiveDialog("GetUserNameDialog");

// Add an intent rule.
getUserNameDialog.AddRule(new IntentRule("GetName", 
    steps: new List<IDialog>() {
        // Add TextInput step. This step will capture user's input and set it to 'user.name' property.
        // See ./memory-model-overview.md for available memory scopes.
        new TextInput()
        {
            Property = "user.name",
            Prompt = new ActivityTemplate("Hi, What is your name?")
        }
}));
```

### ChoiceInput
Choice input asks for a choice from a set of options. 

**Note:** By default, ChoiceInput does not trigger consultation if the choice input recognizer has a high confidence match against provided choices. As an example, if one of your choices were cancel and user said 'cancel' choice input will pick this up although the expected behavior might be for the parent to capture this and handle this as global 'cancel' message from the user (or initiate disambiguation).

``` C#
// Create an adaptive dialog.
var getUserFavoriteColor = new AdaptiveDialog("GetUserColorDialog");
getUserFavoriteColor.AddRule(new IntentRule("GetColor", 
    steps: new List<IDialog>() {
        // Add choice input.
        new ChoiceInput()
        {
            // Output from the user is automatically set to this property
            Property = "user.favColor",
            // List of possible styles supported by choice prompt. 
            Style = ListStyle.Auto,
            Prompt = new ActivityTemplate("What is your favorite color?"),
            Choices = new List<Choice>()
            {
                new Choice("Red"),
                new Choice("Blue"),
                new Choice("Green")
            }
        }
}));
```

### ConfirmInput
As the name implies, asks user for confirmation. 

**Note:** By default, ConfirmInput does not trigger consultation if the confirm input recognizer has a high confidence match against provided response. 

``` C#
// Create adaptive dialog.
var ConfirmationDialog = new AdaptiveDialog("ConfirmationDialog");
// We are using an Event rule here so any other step can just call EmitEvent when it wants to present
// a confirmation input to the user.
ConfirmationDialog.AddRule(new EventRule(["Contoso.travelBot.confirm"], 
    steps: new List<IDialog> {
        // Add confirmation input.
        new ConfirmInput()
        {
            Property = "turn.contoso.travelBot.confirmOutcome",
            // Since this prompt is built as a generic confirmation wrapper, the actual prompt text is 
            // read from a specific memory location. The caller of this dialog needs to set the prompt
            // string to that location before calling the "ConfirmationDialog".
            // All prompts support rich language generation based resolution for output generation. 
            // See ../../language-generation/README.md to learn more.
            Prompt = new ActivityTemplate("{turn.contoso.travelBot.confirmPromptMessage}")
        }
    });
```

### NumberInput

Asks for a number.

**Note:** By default, ConfirmInput does not trigger consultation if the confirm input recognizer has a high confidence match against provided response. 

``` C#
// Create adaptive dialog.
var getNumberDialog = new AdaptiveDialog("ConfirmationDialog");
// We are using an Event rule here so any other step can just call EmitEvent when it wants to collect
// a number from the user.
getNumberDialog.AddRule(new EventRule(["Contoso.travelBot.getNumber"], 
    steps: new List<IDialog> {
        new NumberInput<int>()
        {
            Property = "turn.contoso.travelBot.numberPrompt",
            Prompt = new ActivityTemplate("{turn.contoso.travelBot.numberMessage}")
        }
    });
```

## Steps
_Steps_ help put together the flow of conversation when a specific event is captured via a Rule. **_Note:_** unlike Waterfall dialog where each step is a function, each step in an Adaptive dialog is in itself a dialog. This enables adaptive dialogs by design to 
- have a much cleaner ability to handle and deal with interruptions.
- branch conditionally based on context or state.

Adaptive dialogs support the following steps - 
- Sending a response
    - [SendActivity](#SendActivity)
- Memory manipulation
    - [SaveEntity](#SaveEntity)
    - [EditArray](#EditArray)
    - [InitProperty](#InitProperty)
    - [SetProperty](#SetProperty)
    - [DeleteProperty](#DeleteProperty)
- Conversational flow and dialog management
    - [IfCondition](#IfCondition)
    - [SwitchCondition](#SwitchCondition)
    - [EndTurn](#EndTurn)
    - [BeginDialog](#BeginDialog)
    - [EndDialog](#EndDialog)
    - [CancelAllDialog](#CancelAllDialog)
    - [ReplaceDialog](#ReplaceDialog)
    - [RepeatDialog](#RepeatDialog)
    - [EmitEvent](#EmitEvent)
- Roll your own code
    - [CodeStep](#CodeStep)
    - [HttpRequest](#HttpRequest)
- Tracing and logging
    - [TraceActivity](#TraceActivity)
    - [LogStep](#LogStep)

### SendActivity
Used to send an activity to user. 

``` C#
// Example of a simple SendActivity step
var greetUserDialog = new AdaptiveDialog("greetUserDialog");
greetUserDialog.AddRule(new IntentRule("greetUser", 
    steps: new List<IDialog>() {
        new SendActivity("Hello")
}));

// Example that includees reference to property on bot state.
var greetUserDialog = new AdaptiveDialog("greetUserDialog");
greetUserDialog.AddRule(new IntentRule("greetUser", 
    steps: new List<IDialog>() {
        new TextInput("user.name", "What is your name?"),
        new SendActivity("Hello, {user.name}")
}));
```
See [here][3] to learn more about using language generation instead of hard coding actual response text in SendActivity.

### SaveEntity
Use this step to promte an entity (from a recognizer) into a different memory scope. By default entities from recognizer are available under the [turn scope][4] and the life time of all information under that scope is the end of that turn of conversation. 

``` C#
var greetUserDialog = new AdaptiveDialog("greetUserDialog");
greetUserDialog.AddRule(new IntentRule("greetUser", 
    steps: new List<IDialog>() {
        // Save the userName entitiy from a recognizer.
        new SaveEntity("user.name", "turn.entities.userName[0]"),
        // Ask user for their name. All inputs by default will only initiate a prompt if the property does not exist.
        new TextInput()
        {
            Prompt = new ActivityTemplate("What is your name?"),
            Property = "user.name"
        },
        new SendActivity("Hello, {user.name}")
}));
```

### EditArray
Used to perform edit operations on an array property.

``` C#
var addToDoDialog = new AdaptiveDialog("addToDoDialog");
addToDoDialog.AddRule(new IntentRule("addToDo", 
    steps: new List<IDialog>() {
        // Save the userName entitiy from a recognizer.
        new SaveEntity("dialog.addTodo.title", "turn.entities.todoTitle[0]"),
        new TextInput()
        {
            Prompt = new ActivityTemplate("What is the title of your todo?"),
            Property = "dialog.addTodo.title"
        },
        // Add the current todo to the todo's list for this user.
        new EditArray(EditArray.ArrayChangeType.Push, "user.todos", "dialog.addTodo.title"),
        new SendActivity("Ok, I have added {dialog.addTodo.title} to your todos."),
        new SendActivity("You now have {count(user.todos)} items in your todo.")
}));
```

### InitProperty
Initializes a property in memory. Can either initialize an array or object.

``` C#
new InitProperty()
{
    Property = "user.todos",
    Type = "array" // this can either be "array" or "object"
}
```

### SetProperty 
Used to set a property's value in memory. The value can either be an explict string or an expression. See [here][5] to learn more about the common expressions language.

``` C#
new SetProperty() 
{
    Property = "user.firstName",
    // If user name is Vishwac Kannan, this sets first name to 'Vishwac'
    Value = new ExpressionEngine().Parse("split(user.name, ' ')[0]")
},
```

### DeleteProperty
Removes a property from memory.

``` C#
new DeleteProperty 
{
    Property = "user.firstName"
}
```

### IfCondition
Used to represent branch in the conversational flow based on a specific condition. Conditions are expressed using the common expression language. See [here][5] to learn more about the common expression language.

``` C#
var addToDoDialog = new AdaptiveDialog("addToDoDialog");
addToDoDialog.AddRule(new IntentRule("addToDo",
    steps: new List<IDialog>() {
    // Save the userName entitiy from a recognizer.
    new SaveEntity("dialog.addTodo.title", "turn.entities.todoTitle[0]"),
    new TextInput()
    {
        Prompt = new ActivityTemplate("What is the title of your todo?"),
        Property = "dialog.addTodo.title"
    },
    // Add the current todo to the todo's list for this user.
    new EditArray(EditArray.ArrayChangeType.Push, "user.todos", "dialog.addTodo.title"),
    new SendActivity("Ok, I have added {dialog.addTodo.title} to your todos."),
    new IfCondition()
    {
        Condition = new ExpressionEngine().Parse("toLower(dialog.addTodo.title) == 'call santa'"),
        Steps = new List<IDialog>()
        {
            new SendActivity("Yes master. On it right now [You have unlocked an easter egg] :)")
        }
    },
    new SendActivity("You now have {count(user.todos)} items in your todo.")
}));
```

### SwitchCondition
Used to represent branching in conversational flow based on the outcome of an expression evaluation. See [here][5] to learn more about the common expression language.

``` C#

```

### EndTurn

### BeginDialog

### EndDialog

### CancelAllDialog

### ReplaceDialog

### RepeatDialog

### EmitEvent

### CodeStep

### HttpRequest

### TraceActivity

### LogStep


[1]:https://luis.ai
[2]:https://github.com/Microsoft/BotBuilder/blob/master/specs/botframework-activity/botframework-activity.md#locale
[3]:./language-generation.md
[4]:./memory-model-overview.md#turn-scope
[5]:../../common-expression-language/README.md