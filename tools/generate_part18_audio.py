"""Original MeWGic score and foley, synthesized with Python's standard library."""
from array import array
from math import cos, exp, pi, sin
from pathlib import Path
from random import Random
import sys
import wave

RATE = 24000
TAU = 2 * pi
OUTPUT = Path(__file__).resolve().parents[1] / "Assets/Resources/Audio/Part18"


def write(name, samples):
    path = OUTPUT / f"{name}.wav"
    pcm = array("h", (round(max(-1, min(1, v)) * 32767) for v in samples))
    if sys.byteorder != "little":
        pcm.byteswap()
    with wave.open(str(path), "wb") as stream:
        stream.setparams((1, 2, RATE, 0, "NONE", "not compressed"))
        stream.writeframes(pcm.tobytes())


def notes(name, duration, events):
    samples = [0.] * round(duration * RATE)
    for start, midi, length, strength, bell in events:
        frequency = 440 * 2 ** ((midi - 69) / 12)
        phase = TAU * frequency
        for i in range(round(length * RATE)):
            t = i / RATE
            envelope = min(1, t / (.009 if bell else .20), (length - t) / (.09 if bell else .28))
            signal = sin(phase * t)
            if bell:
                signal = (signal + .27 * sin(phase * 2.01 * t) +
                          .11 * sin(phase * 3.97 * t)) * exp(-3 * t / length)
            else:
                signal = (signal + .16 * sin(phase * 2 * t)) * (.95 + .05 * cos(TAU * 2 * t))
            samples[(round(start * RATE) + i) % len(samples)] += signal * envelope * strength
    write(name, samples)


def music(name, beat, chords, melody):
    events = []
    for bar, chord in enumerate(chords):
        start = bar * 4 * beat
        for midi in chord:
            events.append((start, midi, 4.5 * beat, .058, False))
        for step, midi in enumerate(melody[bar]):
            if midi:
                events.append((start + step * .5 * beat, midi, 1.7 * beat, .22, True))
    notes(name, 16 * beat, events)


def noise(name, duration, seed, pour=False):
    rng = Random(seed)
    slow = softer = 0.
    samples = []
    for i in range(round(duration * RATE)):
        t = i / RATE
        white = rng.uniform(-1, 1)
        slow += .15 * (white - slow)
        softer += .013 * (slow - softer)
        bed = slow * .38 + softer * .58
        if pour:
            shape = min(1, t / .035, (duration - t) / .09)
            bubbles = .048 * sin(TAU * (360 * t + 620 * t * t)) * (.5 + .5 * cos(TAU * 21 * t)) ** 3
            samples.append(shape * (bed + bubbles))
        else:
            shape = min(1, t / .12, (duration - t) / .12)
            samples.append(shape * bed * (.82 + .18 * cos(TAU * t / duration)))
    write(name, samples)


def chime(name, duration, motif, volume=.33, length=.32, step=.075):
    notes(name, duration, [(i * step, midi, length, volume, True) for i, midi in enumerate(motif)])


if __name__ == "__main__":
    OUTPUT.mkdir(parents=True, exist_ok=True)
    music("HomeMusic", 1.0, [(48,55,64), (45,52,60), (41,48,57), (43,50,62)],
          [(72,0,76,79,0,76,74,0), (72,0,69,76,0,72,0,0),
           (77,0,81,0,76,72,0,74), (74,0,79,0,76,74,71,0)])
    music("PuzzleMusic", 1.125, [(50,57,64), (46,53,62), (48,55,64), (45,52,59)],
          [(69,0,0,74,0,0,76,0), (77,0,0,74,0,0,69,0),
           (67,0,0,72,0,0,76,0), (76,0,0,71,0,0,69,0)])
    noise("Waterfall", 6, 1823)
    noise("Pour", .36, 1807, True)
    chime("Valid", .28, [76, 81], .24, .19, .06)
    chime("Invalid", .34, [60, 54], .27, .22, .08)
    chime("Victory", 1.28, [67, 72, 76, 79, 84], .37, .60, .13)
    chime("Coin", .55, [84, 88, 91], .22, .28, .06)
    chime("Chest", .90, [54, 61, 73, 80], .33, .47, .11)
    chime("Map", .72, [65, 72, 77, 81], .26, .36, .09)
