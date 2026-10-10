-- Floats every tiled window and restores them on the next toggle,
-- mirroring yabai's `--layout float` and komorebi's `toggle-tiling`.

local FLOAT_ACTION = {
    ENABLE  = "enable",
    DISABLE = "disable",
}

local TILED_WINDOWS_FILTER    = { floating = false }
local ADDRESS_SELECTOR_PREFIX = "address:"

local TilingToggle = {}
TilingToggle.__index = TilingToggle

function TilingToggle.new()
    return setmetatable({ floatedAddresses = {} }, TilingToggle)
end

local function setFloating(window, action)
    hl.dispatch(hl.dsp.window.float({ window = window, action = action }))
end

function TilingToggle:isFloatingAll()
    return #self.floatedAddresses > 0
end

function TilingToggle:floatAll()
    for _, window in ipairs(hl.get_windows(TILED_WINDOWS_FILTER)) do
        table.insert(self.floatedAddresses, window.address)
        setFloating(window, FLOAT_ACTION.ENABLE)
    end
end

function TilingToggle:restoreAll()
    for _, address in ipairs(self.floatedAddresses) do
        local window = hl.get_window(ADDRESS_SELECTOR_PREFIX .. address)
        if window ~= nil then
            setFloating(window, FLOAT_ACTION.DISABLE)
        end
    end
    self.floatedAddresses = {}
end

function TilingToggle:toggle()
    if self:isFloatingAll() then
        self:restoreAll()
    else
        self:floatAll()
    end
end

function TilingToggle:handler()
    return function() self:toggle() end
end

return TilingToggle
