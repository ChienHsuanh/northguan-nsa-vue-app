<script setup lang="ts">
import type { DateValue } from "@internationalized/date"
import type { CalendarRootEmits, CalendarRootProps } from "reka-ui"
import type { HTMLAttributes } from "vue"
import { getLocalTimeZone, today } from "@internationalized/date"
import { useVModel } from "@vueuse/core"
import { CalendarRoot, useDateFormatter, useForwardPropsEmits } from "reka-ui"
import { createDecade, createYear, toDate } from "reka-ui/date"
import { computed } from "vue"
import { cn } from "@/lib/utils"
import { Button } from "@/components/ui/button"
import {
  Popover,
  PopoverContent,
  PopoverTrigger,
} from "@/components/ui/popover"
import { CalendarCell, CalendarCellTrigger, CalendarGrid, CalendarGridBody, CalendarGridHead, CalendarGridRow, CalendarHeadCell, CalendarHeader, CalendarHeading } from "@/components/ui/calendar"
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select"
import { Calendar as CalendarIcon } from "lucide-vue-next"

interface Props extends CalendarRootProps {
  class?: HTMLAttributes["class"]
  placeholder?: string
}

const props = withDefaults(defineProps<Props>(), {
  modelValue: undefined,
  placeholder: "選擇日期",
  weekdayFormat: "short",
})

const emits = defineEmits<CalendarRootEmits>()

const delegatedProps = computed(() => {
  const { class: _, placeholder: __, ...delegated } = props
  return delegated
})

const modelValue = useVModel(props, "modelValue", emits, {
  passive: true,
  defaultValue: today('Asia/Taipei'),
})

const forwarded = useForwardPropsEmits(delegatedProps, emits)

const formatter = useDateFormatter("zh-TW", { timeZone: 'Asia/Taipei' })

const displayText = computed(() => {
  if (modelValue.value) {
    return formatter.custom(toDate(modelValue.value, 'Asia/Taipei'), {
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
            'w-full justify-start text-left font-normal',
            !modelValue && 'text-muted-foreground',
            props.class
          )
        "
      >
        <CalendarIcon class="mr-2 h-4 w-4" />
        {{ displayText }}
      </Button>
    </PopoverTrigger>
    <PopoverContent class="w-auto p-0">
      <CalendarRoot
        v-slot="{ date, grid, weekDays }"
        v-model="modelValue"
        v-bind="forwarded"
        class="rounded-md border p-3"
      >
        <CalendarHeader>
          <CalendarHeading class="flex w-full items-center justify-between gap-2">
            <Select
              :default-value="modelValue?.month.toString()"
              @update:model-value="(v) => {
                if (!v || !modelValue) return;
                if (Number(v) === modelValue?.month) return;
                modelValue = modelValue.set({
                  month: Number(v),
                })
              }"
            >
              <SelectTrigger aria-label="選擇月份" class="w-[60%]">
                <SelectValue placeholder="選擇月份" />
              </SelectTrigger>
              <SelectContent class="max-h-[200px]">
                <SelectItem
                  v-for="month in createYear({ dateObj: date })"
                  :key="month.toString()" 
                  :value="month.month.toString()"
                >
                  {{ formatter.custom(toDate(month), { month: 'long' }) }}
                </SelectItem>
              </SelectContent>
            </Select>

            <Select
              :default-value="modelValue?.year.toString()"
              @update:model-value="(v) => {
                if (!v || !modelValue) return;
                if (Number(v) === modelValue?.year) return;
                modelValue = modelValue.set({
                  year: Number(v),
                })
              }"
            >
              <SelectTrigger aria-label="選擇年份" class="w-[40%]">
                <SelectValue placeholder="選擇年份" />
              </SelectTrigger>
              <SelectContent class="max-h-[200px]">
                <SelectItem
                  v-for="yearValue in createDecade({ dateObj: date, startIndex: -10, endIndex: 10 })"
                  :key="yearValue.toString()" 
                  :value="yearValue.year.toString()"
                >
                  {{ yearValue.year }}
                </SelectItem>
              </SelectContent>
            </Select>
          </CalendarHeading>
        </CalendarHeader>

        <div class="flex flex-col space-y-4 pt-4">
          <CalendarGrid v-for="month in grid" :key="month.value.toString()">
            <CalendarGridHead>
              <CalendarGridRow>
                <CalendarHeadCell
                  v-for="day in weekDays" 
                  :key="day"
                >
                  {{ day }}
                </CalendarHeadCell>
              </CalendarGridRow>
            </CalendarGridHead>
            <CalendarGridBody class="grid">
              <CalendarGridRow 
                v-for="(weekDates, index) in month.rows" 
                :key="`weekDate-${index}`" 
                class="mt-2 w-full"
              >
                <CalendarCell
                  v-for="weekDate in weekDates"
                  :key="weekDate.toString()"
                  :date="weekDate"
                >
                  <CalendarCellTrigger
                    :day="weekDate"
                    :month="month.value"
                  />
                </CalendarCell>
              </CalendarGridRow>
            </CalendarGridBody>
          </CalendarGrid>
        </div>
      </CalendarRoot>
    </PopoverContent>
  </Popover>
</template>