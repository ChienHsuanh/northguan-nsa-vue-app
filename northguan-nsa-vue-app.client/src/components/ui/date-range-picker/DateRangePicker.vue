<script setup lang="ts">
import type { DateValue } from "@internationalized/date"
import type { DateRange } from "reka-ui"
import type { Grid } from "reka-ui/date"
import type { Ref } from "vue"
import {
  CalendarDate,
  isEqualMonth,
} from "@internationalized/date"
import {
  Calendar as CalendarIcon,
  ChevronLeft,
  ChevronRight,
} from "lucide-vue-next"
import { RangeCalendarRoot, useDateFormatter } from "reka-ui"
import { createMonth, toDate } from "reka-ui/date"
import { ref, watch, computed } from "vue"
import { cn } from "@/lib/utils"
import { Button, buttonVariants } from "@/components/ui/button"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import {
  RangeCalendarCell,
  RangeCalendarCellTrigger,
  RangeCalendarGrid,
  RangeCalendarGridBody,
  RangeCalendarGridHead,
  RangeCalendarGridRow,
  RangeCalendarHeadCell,
} from "@/components/ui/range-calendar"

interface Props {
  modelValue?: DateRange
  placeholder?: string
  class?: string
}

interface Emits {
  (e: 'update:modelValue', value: DateRange): void
}

const props = withDefaults(defineProps<Props>(), {
  placeholder: '選擇日期範圍',
  class: 'w-[280px]'
})

const emit = defineEmits<Emits>()

// 預設值：最近7天
const today = new CalendarDate(new Date().getFullYear(), new Date().getMonth() + 1, new Date().getDate())
const lastWeek = today.subtract({ days: 7 })

const value = computed({
  get: () => props.modelValue || {
    start: lastWeek,
    end: today,
  },
  set: (newValue) => emit('update:modelValue', newValue)
}) as Ref<DateRange>

const locale = ref("zh-TW")
const formatter = useDateFormatter(locale.value, { timeZone: 'Asia/Taipei' })

const placeholder = ref(value.value.start) as Ref<DateValue>
const secondMonthPlaceholder = ref(value.value.end) as Ref<DateValue>

const firstMonth = ref(
  createMonth({
    dateObj: placeholder.value,
    locale: locale.value,
    fixedWeeks: true,
    weekStartsOn: 0,
  }),
) as Ref<Grid<DateValue>>

const secondMonth = ref(
  createMonth({
    dateObj: secondMonthPlaceholder.value,
    locale: locale.value,
    fixedWeeks: true,
    weekStartsOn: 0,
  }),
) as Ref<Grid<DateValue>>

function updateMonth(reference: "first" | "second", months: number) {
  if (reference === "first") {
    placeholder.value = placeholder.value.add({ months })
  } else {
    secondMonthPlaceholder.value = secondMonthPlaceholder.value.add({
      months,
    })
  }
}

watch(placeholder, (_placeholder) => {
  firstMonth.value = createMonth({
    dateObj: _placeholder,
    weekStartsOn: 0,
    fixedWeeks: false,
    locale: locale.value,
  })
  if (isEqualMonth(secondMonthPlaceholder.value, _placeholder)) {
    secondMonthPlaceholder.value = secondMonthPlaceholder.value.add({
      months: 1,
    })
  }
})

watch(secondMonthPlaceholder, (_secondMonthPlaceholder) => {
  secondMonth.value = createMonth({
    dateObj: _secondMonthPlaceholder,
    weekStartsOn: 0,
    fixedWeeks: false,
    locale: locale.value,
  })
  if (isEqualMonth(_secondMonthPlaceholder, placeholder.value))
    placeholder.value = placeholder.value.subtract({ months: 1 })
})

// 格式化顯示文字
const displayText = computed(() => {
  if (value.value.start && value.value.end) {
    return `${formatter.custom(toDate(value.value.start, 'Asia/Taipei'), {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    })} - ${formatter.custom(toDate(value.value.end, 'Asia/Taipei'), {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    })}`
  } else if (value.value.start) {
    return formatter.custom(toDate(value.value.start, 'Asia/Taipei'), {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
    })
  }
  return props.placeholder
})
</script>

<template>
  <Popover>
    <PopoverTrigger as-child>
      <Button
        variant="outline"
        :class="
          cn(
            'justify-start text-left font-normal',
            !value.start && 'text-muted-foreground',
            props.class
          )
        "
      >
        <CalendarIcon class="mr-2 h-4 w-4" />
        {{ displayText }}
      </Button>
    </PopoverTrigger>
    <PopoverContent class="w-auto p-0">
      <RangeCalendarRoot 
        v-slot="{ weekDays }" 
        v-model="value" 
        v-model:placeholder="placeholder" 
        class="p-3"
      >
        <div class="flex flex-col gap-y-4 mt-4 sm:flex-row sm:gap-x-4 sm:gap-y-0">
          <div class="flex flex-col gap-4">
            <div class="flex items-center justify-between">
              <button
                :class="
                  cn(
                    buttonVariants({ variant: 'outline' }),
                    'h-7 w-7 bg-transparent p-0 opacity-50 hover:opacity-100',
                  )
                "
                @click="updateMonth('first', -1)"
              >
                <ChevronLeft class="h-4 w-4" />
              </button>
              <div :class="cn('text-sm font-medium')">
                {{
                  formatter.custom(toDate(firstMonth.value), {
                    year: 'numeric',
                    month: 'long',
                  })
                }}
              </div>
              <button
                :class="
                  cn(
                    buttonVariants({ variant: 'outline' }),
                    'h-7 w-7 bg-transparent p-0 opacity-50 hover:opacity-100',
                  )
                "
                @click="updateMonth('first', 1)"
              >
                <ChevronRight class="h-4 w-4" />
              </button>
            </div>
            <RangeCalendarGrid>
              <RangeCalendarGridHead>
                <RangeCalendarGridRow>
                  <RangeCalendarHeadCell
                    v-for="day in weekDays"
                    :key="day"
                    class="w-full"
                  >
                    {{ day }}
                  </RangeCalendarHeadCell>
                </RangeCalendarGridRow>
              </RangeCalendarGridHead>
              <RangeCalendarGridBody>
                <RangeCalendarGridRow
                  v-for="(weekDates, index) in firstMonth.rows"
                  :key="`weekDate-${index}`"
                  class="mt-2 w-full"
                >
                  <RangeCalendarCell
                    v-for="weekDate in weekDates"
                    :key="weekDate.toString()"
                    :date="weekDate"
                  >
                    <RangeCalendarCellTrigger
                      :day="weekDate"
                      :month="firstMonth.value"
                    />
                  </RangeCalendarCell>
                </RangeCalendarGridRow>
              </RangeCalendarGridBody>
            </RangeCalendarGrid>
          </div>
          <div class="flex flex-col gap-4">
            <div class="flex items-center justify-between">
              <button
                :class="
                  cn(
                    buttonVariants({ variant: 'outline' }),
                    'h-7 w-7 bg-transparent p-0 opacity-50 hover:opacity-100',
                  )
                "
                @click="updateMonth('second', -1)"
              >
                <ChevronLeft class="h-4 w-4" />
              </button>
              <div :class="cn('text-sm font-medium')">
                {{
                  formatter.custom(toDate(secondMonth.value), {
                    year: 'numeric',
                    month: 'long',
                  })
                }}
              </div>
              <button
                :class="
                  cn(
                    buttonVariants({ variant: 'outline' }),
                    'h-7 w-7 bg-transparent p-0 opacity-50 hover:opacity-100',
                  )
                "
                @click="updateMonth('second', 1)"
              >
                <ChevronRight class="h-4 w-4" />
              </button>
            </div>
            <RangeCalendarGrid>
              <RangeCalendarGridHead>
                <RangeCalendarGridRow>
                  <RangeCalendarHeadCell
                    v-for="day in weekDays"
                    :key="day"
                    class="w-full"
                  >
                    {{ day }}
                  </RangeCalendarHeadCell>
                </RangeCalendarGridRow>
              </RangeCalendarGridHead>
              <RangeCalendarGridBody>
                <RangeCalendarGridRow
                  v-for="(weekDates, index) in secondMonth.rows"
                  :key="`weekDate-${index}`"
                  class="mt-2 w-full"
                >
                  <RangeCalendarCell
                    v-for="weekDate in weekDates"
                    :key="weekDate.toString()"
                    :date="weekDate"
                  >
                    <RangeCalendarCellTrigger
                      :day="weekDate"
                      :month="secondMonth.value"
                    />
                  </RangeCalendarCell>
                </RangeCalendarGridRow>
              </RangeCalendarGridBody>
            </RangeCalendarGrid>
          </div>
        </div>
      </RangeCalendarRoot>
    </PopoverContent>
  </Popover>
</template>