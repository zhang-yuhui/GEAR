using UnityEngine;

public class TimeController : MonoBehaviour
{
    // Playback control variables
    public float playbackSpeed = 1.0f; // Speed multiplier (1x, 2x, etc.)
    public bool isPlaying = false;      // Playback state
    private bool isShown = false;
    private float currentTime = 0f;    // Current simulation time

    // Delegate and event for time updates
    public delegate void TimeUpdated(float newTime);
    public event TimeUpdated OnTimeUpdated;

    public delegate void FrameUpdated(int newFrame);
    public event FrameUpdated OnFrameUpdated;
    public float frameRate = 1f;
    private float timeElapsed = 0f;
    private int currentFrame = 0; // starts at 0;
    public int maxFrame = 0; // always larger than currentFrame
    void Update()
    {
        if(maxFrame <= 0)
            return;
        if(!isPlaying && !isShown){
            OnFrameUpdated?.Invoke(currentFrame);
            isShown = true;
            OnFrameUpdated?.Invoke(currentFrame);
        }
        // click the space key to play or pause
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (isPlaying)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            currentFrame = 0;
            OnFrameUpdated?.Invoke(currentFrame);
        }

        if(!isPlaying){
            if(Input.GetKeyDown(KeyCode.RightArrow)){
                NextFrame();
                OnFrameUpdated?.Invoke(currentFrame);
            }
            if(Input.GetKeyDown(KeyCode.LeftArrow)){
                PreviousFrame();
                OnFrameUpdated?.Invoke(currentFrame);
            }
                
        }

        if (isPlaying)
        {
            // Increment currentTime based on deltaTime and playbackSpeed
            currentTime += Time.deltaTime * playbackSpeed;
            timeElapsed += Time.deltaTime;
            if (timeElapsed >= 1f / frameRate)
            {
                OnFrameUpdated?.Invoke(currentFrame);
                NextFrame();
                timeElapsed = 0;
            }
            // Trigger the time update event
            OnTimeUpdated?.Invoke(currentTime);
        }
    }

    // Public methods to control time
    public void SetTime(float time)
    {
        currentTime = time;
        OnTimeUpdated?.Invoke(currentTime); // Notify subscribers of the time change
    }

    public float GetCurrentTime()
    {
        return currentTime;
    }

    public void Play()
    {
        isPlaying = true;
    }

    public void Pause()
    {
        isPlaying = false;
    }

    public void SetPlaybackSpeed(float speed)
    {
        playbackSpeed = speed;
    }

    public int NextFrame(){
        currentFrame ++;
        currentFrame = (currentFrame + maxFrame) % maxFrame;
        return currentFrame;
    }
    public int PreviousFrame(){
        currentFrame --;
        currentFrame = (currentFrame + maxFrame) % maxFrame;
        return currentFrame;
    }
}