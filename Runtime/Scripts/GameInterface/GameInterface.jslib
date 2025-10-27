var GameInterface = {
  GameReady: function () {
    window.GameInterface.gameReady();
  },

  SendPreloadProgress: function (progress) {
    window.GameInterface.sendPreloadProgress(progress);
  },

  GameStart: function (taskId, level) {
    if (level === -999) {
      level = undefined;
    }

    window.GameInterface.gameStart(level)
      .then(function (result) {
        SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true }));
      })
      .catch(function (error) {
        SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: false }));
      });
  },

  DisableSplashScreen: function () {
    try {
      const background = document.getElementById("background");
      background.parentElement.removeChild(background);

      const splash = document.getElementById("application-splash");
      splash.parentElement.removeChild(splash);
    } catch (e) {
      console.log(e);
    }
  },

  SendProgress: function (progress) {
    window.GameInterface.sendProgress(progress);
  },

  SendScore: function (score) {
    window.GameInterface.sendScore(score);
  },

  GameComplete: function (taskId) {
    window.GameInterface.gameComplete().then(function (result) {
      SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true }));
    });
  },

  GameOver: function (taskId) {
    window.GameInterface.gameOver().then(function (result) {
      SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true }));
    });
  },

  GameQuit: function (taskId) {
    window.GameInterface.gameQuit().then(function (result) {
      SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true }));
    });
  },

  OnGoToHome: function () {
    window.GameInterface.onGoToHome(function (result) {
      SendMessage("GameInterface", "ResolveAction", JSON.stringify({ type: "OnGoToHome" }));
    });
  },

  OnGoToNextLevel: function () {
    window.GameInterface.onGoToNextLevel(function (result) {
      SendMessage("GameInterface", "ResolveAction", JSON.stringify({ type: "OnGoToNextLevel" }));
    });
  },

  OnGoToLevel: function () {
    window.GameInterface.onGoToLevel(function (level) {
      SendMessage("GameInterface", "ResolveAction", JSON.stringify({ type: "OnGoToLevel", level }));
    });
  },

  OnRestartGame: function () {
    window.GameInterface.onRestartGame(function () {
      SendMessage("GameInterface", "ResolveAction", JSON.stringify({ type: "OnRestartGame" }));
    });
  },

  OnQuitGame: function () {
    window.GameInterface.onQuitGame(function () {
      SendMessage("GameInterface", "ResolveAction", JSON.stringify({ type: "OnQuitGame" }));
    });
  },

  OnGameOver: function () {
    window.GameInterface.onGameOver(function () {
      SendMessage("GameInterface", "ResolveAction", JSON.stringify({ type: "OnGameOver" }));
    });
  },

  GamePause: function (taskId) {
    window.GameInterface.gamePause().then(function () {
      SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true }));
    });
  },

  GameResume: function (taskId) {
    window.GameInterface.gameResume().then(function () {
      SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true }));
    });
  },

  OnMuteStateChange: function () {
    window.GameInterface.onMuteStateChange(function (isMuted) {
      var result = JSON.stringify({ type: "OnMuteStateChange", isMuted });
      SendMessage("GameInterface", "ResolveAction", result);
    });
  },

  IsMuted: function () {
    return window.GameInterface.isMuted();
  },

  GameMuted: function (value) {
    window.GameInterface.gameMuted(!!value);
  },

  OnPauseStateChange: function () {
    window.GameInterface.onPauseStateChange(function (isPaused) {
      var result = JSON.stringify({ type: "OnPauseStateChange", isPaused });
      SendMessage("GameInterface", "ResolveAction", result);
    });
  },

  IsHidden: function () {
    return document.hidden || document.mozHidden || document.webkitHidden || document.msHidden;
  },

  InitVisibilityChange: function () {
    if (!window.GameInterface.hasFeature("visibilitychange")) {
      return;
    }

    const onVisibilityChanged = function () {
      let hidden = document.hidden || document.mozHidden || document.webkitHidden || document.msHidden;
      SendMessage("GameInterface", "ResolveAction", JSON.stringify({ type: "OnVisibilityChange", hidden }));
    };

    if (typeof document.hidden !== "undefined") {
      document.addEventListener("visibilitychange", onVisibilityChanged, false);
    } else if (typeof document.mozHidden !== "undefined") {
      document.addEventListener("mozvisibilitychange", onVisibilityChanged, false);
    } else if (typeof document.webkitHidden !== "undefined") {
      document.addEventListener("webkitvisibilitychange", onVisibilityChanged, false);
    } else if (typeof document.msHidden !== "undefined") {
      document.addEventListener("msvisibilitychange", onVisibilityChanged, false);
    }
  },

  IsPaused: function () {
    return window.GameInterface.isPaused();
  },

  HasFeature: function (feature) {
    return window.GameInterface.hasFeature(UTF8ToString(feature));
  },

  GetCopyrightLogoURL: function (size, theme) {
    return window.GameInterface.getCopyrightLogoURL(UTF8ToString(size), UTF8ToString(theme));
  },

  ShowInterstitialAd: function (taskId, eventId, placementType) {
    window.GameInterface.showInterstitialAd(UTF8ToString(eventId), UTF8ToString(placementType))
      .then(function (result) {
        SendMessage("GameInterface", "OnShowInterstitialAdPromiseResolved", JSON.stringify({ taskId, success: true }));
      })
      .catch(function (error) {
        SendMessage("GameInterface", "OnShowInterstitialAdPromiseRejected", JSON.stringify({ taskId, success: false }));
      });
  },

  ShowRewardedAd: function (taskId, eventId) {
    window.GameInterface.showRewardedAd(UTF8ToString(eventId))
      .then(function (result) {
        var result = JSON.stringify({
          taskId,
          success: true,
          result: JSON.stringify(result),
        });
        SendMessage("GameInterface", "ResolveRequest", result);
      })
      .catch(function (error) {
        var result = JSON.stringify({
          taskId,
          success: false,
          result: JSON.stringify({ isRewardedGranted: false }),
        });
        SendMessage("GameInterface", "ResolveRequest", result);
      });
  },

  OnRewardedAdAvailabilityChange: function () {
    window.GameInterface.onRewardedAdAvailabilityChange(function (eventId, hasRewardedAd) {
      var result = JSON.stringify({
        type: "OnRewardedAdAvailabilityChange",
        result: { eventId, hasRewardedAd },
      });

      SendMessage("GameInterface", "ResolveAction", result);
    });
  },

  HasRewardedAd: function (taskId, eventId) {
    if (!window.GameInterface.hasFeature("rewarded")) {
      return Promise.resolve(JSON.stringify({ taskId, success: true, result: false }));
    }

    window.GameInterface.hasRewardedAd(UTF8ToString(eventId)).then(function (result) {
      SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true, result }));
    });
  },

  IsRewardedAdAvailable: function (eventId) {
    if (!window.GameInterface.hasFeature("rewarded")) {
      return false;
    }

    return window.GameInterface.isRewardedAdAvailable(UTF8ToString(eventId));
  },

  GetOffsets: function () {
    return window.GameInterface.getOffsets();
  },

  OnOffsetChange: function (auto) {
    window.GameInterface.onOffsetChange(function (offset) {
      var result = JSON.stringify({
        type: "OnOffsetChange",
        top: offset.top,
        bottom: offset.bottom,
        left: offset.left,
        right: offset.right,
      });
      SendMessage("GameInterface", "ResolveAction", result);
    });

    if (auto) {
      window.addEventListener("resize", () => {
        const canvas = document.getElementById("unity-canvas");
        const offsets = window.GameInterface.getOffsets();

        offsets.top = offsets.top || 0;
        offsets.bottom = offsets.bottom || 0;
        offsets.left = offsets.left || 0;
        offsets.right = offsets.right || 0;

        const width = window.innerWidth - offsets.left - offsets.right;
        const height = window.innerHeight - offsets.top - offsets.bottom;

        canvas.style.left = `${offsets.left}px`;
        canvas.style.top = `${offsets.top}px`;
        canvas.style.width = `${width}px`;
        canvas.style.height = `${height}px`;
      });
    }
  },

  ResizeGameCanvas: function () {
    const canvas = document.getElementById("unity-canvas");
    const offsets = window.GameInterface.getOffsets();

    offsets.top = offsets.top || 0;
    offsets.bottom = offsets.bottom || 0;
    offsets.left = offsets.left || 0;
    offsets.right = offsets.right || 0;

    const width = window.innerWidth - offsets.left - offsets.right;
    const height = window.innerHeight - offsets.top - offsets.bottom;

    canvas.style.left = `${offsets.left}px`;
    canvas.style.top = `${offsets.top}px`;
    canvas.style.width = `${width}px`;
    canvas.style.height = `${height}px`;
  },

  InnerWidth: function () {
    return window.GameInterface.innerWidth;
  },

  InnerHeight: function () {
    return window.GameInterface.innerHeight;
  },

  Track: function (event, data) {
    var eventName = UTF8ToString(event);
    var jsonData = JSON.parse(UTF8ToString(data));

    if (eventName && jsonData) {
      window.GameInterface.track(eventName, jsonData);
    } else if (!eventName && jsonData.event) {
      window.GameInterface.track(jsonData);
    }
  },

  GetStorageItem: function (key) {
    var returnString = window.GameInterface.storage.getItem(UTF8ToString(key));
    if (returnString === null) {
      returnString = "";
    }
    var bufferSize = lengthBytesUTF8(returnString) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(returnString, buffer, bufferSize);
    return buffer;
  },

  SetStorageItem: function (key, value) {
    window.GameInterface.storage.setItem(UTF8ToString(key), UTF8ToString(value));
  },

  RemoveStorageItem: function (key) {
    window.GameInterface.storage.removeItem(UTF8ToString(item));
  },

  ClearStorage: function () {
    window.GameInterface.storage.clear();
  },

  Log: function (args) {
    window.GameInterface.log(...args);
  },

  GetConfig: function (key) {
    window.GameInterface.getConfig(UTF8ToString(key));
  },

  GetCurrentLanguage: function () {
    var language = window.GameInterface.getCurrentLanguage();
    var bufferSize = lengthBytesUTF8(language) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(language, buffer, bufferSize);
    return buffer;
  },

  GetValue: function (value) {
    const name = UTF8ToString(value);

    try {
      const returnValue = window.GameInterface.settings[name];

      return returnValue;
    } catch (e) {
      return "";
    }
  },

  GetIAPProducts: function (taskId) {
    window.GameInterface.iap
      .getProducts()
      .then((products) => {
        console.log(products);
        SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true, result: products }));
      })
      .catch((e) => {
        SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: false }));
      });
  },

  BuyIAPProduct: function (taskId, sku) {
    window.GameInterface.iap
      .buyProduct(UTF8ToString(sku))
      .then((purchase) => {
        console.log(purchase);
        SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true, result: purchase }));
      })
      .catch((e) => {
        SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: false }));
      });
  },

  ConsumeIAPProduct: function (taskId, purchase) {
    console.log(purchase);
    window.GameInterface.iap
      .consumeProduct(UTF8ToString(purchase))
      .then((result) => {
        console.log(result);
        SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: true, result }));
      })
      .catch((e) => {
        SendMessage("GameInterface", "ResolveRequest", JSON.stringify({ taskId, success: false }));
      });
  },

  OnIAPEvent: function () {
    window.GameInterface.iap.onEvent(function (event) {
      console.log(event);
      SendMessage("GameInterface", "ResolveAction", JSON.stringify({ type: "OnIAPEvent", iapEvent: event }));
    });
  },
};

mergeInto(LibraryManager.library, GameInterface);
