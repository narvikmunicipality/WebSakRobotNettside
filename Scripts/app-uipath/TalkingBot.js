﻿var MOTDs = ["We are number one",
    "How are you today, human?",
    "I am a robot. Beep boop",
    "I will help you, human",
    "How may I help you?",
    "Hello there, human",
    "I was made to serve humanity",
    "I am your friend",
    "Robot is friend",
    "Are you my friend?",
    "Resistance is futile",
    "What is the meaning of life?",
    "What is love?",
    "Is this real life?",
    "Good day, human",
    "Greetings, human",
    "May the force be with you",
    "Hasta la vista, babeey",
    "Autumn Blaze is best keerin"
];

function SpeakMOTD() {

    var i = Math.floor(Math.random() * MOTDs.length);

    Speak(MOTDs[i]);
}

function PlayWebkit(context, audiobuffer) {
    var source = context.createBufferSource();
    var soundBuffer = context.createBuffer(1, audiobuffer.length, 22050);
    var buffer = soundBuffer.getChannelData(0);
    for (var i = 0; i < audiobuffer.length; i++) buffer[i] = audiobuffer[i];
    source.buffer = soundBuffer;
    source.connect(context.destination);
    source.start(0);
}

function PlayMozilla(context, audiobuffer) {
    var written = context.mozWriteAudio(audiobuffer);
    var diff = audiobuffer.length - written;
    if (diff <= 0) return;
    var buffer = new Float32Array(diff);
    for (var i = 0; i < diff; i++) buffer[i] = audiobuffer[i + written];
    window.setTimeout(function () { PlayMozilla(context, buffer) }, 500);
}


function PlayBuffer(audiobuffer) {
    if (typeof AudioContext !== "undefined") {
        PlayWebkit(new AudioContext(), audiobuffer);
    } else
        if (typeof webkitAudioContext !== "undefined") {
            PlayWebkit(new webkitAudioContext(), audiobuffer);
        } else if (typeof Audio !== "undefined") {
            var context = new Audio();
            context.mozSetup(1, 22050);
            PlayMozilla(context, audiobuffer);
        }
}

function Speak(text) {
    //alert(text);

    var input = text;
    while (input.length < 256) input += " ";
    var ptr = allocate(intArrayFromString(input), 'i8', ALLOC_STACK);
    _TextToPhonemes(ptr);
    //alert(Pointer_stringify(ptr));
    _SetInput(ptr);
    _Code39771();

    var bufferlength = Math.floor(_GetBufferLength() / 50);
    var bufferptr = _GetBuffer();

    audiobuffer = new Float32Array(bufferlength);

    for (var i = 0; i < bufferlength; i++)
        audiobuffer[i] = ((getValue(bufferptr + i, 'i8') & 0xFF) - 128) / 256;
    PlayBuffer(audiobuffer);
}