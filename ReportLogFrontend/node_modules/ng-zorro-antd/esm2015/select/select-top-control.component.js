/**
 * Use of this source code is governed by an MIT-style license that can be
 * found in the LICENSE file at https://github.com/NG-ZORRO/ng-zorro-antd/blob/master/LICENSE
 */
import { BACKSPACE } from '@angular/cdk/keycodes';
import { ChangeDetectionStrategy, Component, EventEmitter, Host, Input, Optional, Output, ViewChild, ViewEncapsulation } from '@angular/core';
import { zoomMotion } from 'ng-zorro-antd/core/animation';
import { NzNoAnimationDirective } from 'ng-zorro-antd/core/no-animation';
import { NzSelectSearchComponent } from './select-search.component';
export class NzSelectTopControlComponent {
    constructor(noAnimation) {
        this.noAnimation = noAnimation;
        this.showSearch = false;
        this.placeHolder = null;
        this.open = false;
        this.maxTagCount = Infinity;
        this.autofocus = false;
        this.disabled = false;
        this.mode = 'default';
        this.customTemplate = null;
        this.maxTagPlaceholder = null;
        this.removeIcon = null;
        this.listOfTopItem = [];
        this.tokenSeparators = [];
        this.tokenize = new EventEmitter();
        this.inputValueChange = new EventEmitter();
        this.animationEnd = new EventEmitter();
        this.deleteItem = new EventEmitter();
        this.listOfSlicedItem = [];
        this.isShowPlaceholder = true;
        this.isShowSingleLabel = false;
        this.isComposing = false;
        this.inputValue = null;
    }
    onHostKeydown(e) {
        const inputValue = e.target.value;
        if (e.keyCode === BACKSPACE && this.mode !== 'default' && !inputValue && this.listOfTopItem.length > 0) {
            e.preventDefault();
            this.onDeleteItem(this.listOfTopItem[this.listOfTopItem.length - 1]);
        }
    }
    updateTemplateVariable() {
        const isSelectedValueEmpty = this.listOfTopItem.length === 0;
        this.isShowPlaceholder = isSelectedValueEmpty && !this.isComposing && !this.inputValue;
        this.isShowSingleLabel = !isSelectedValueEmpty && !this.isComposing && !this.inputValue;
    }
    isComposingChange(isComposing) {
        this.isComposing = isComposing;
        this.updateTemplateVariable();
    }
    onInputValueChange(value) {
        if (value !== this.inputValue) {
            this.inputValue = value;
            this.updateTemplateVariable();
            this.inputValueChange.emit(value);
            this.tokenSeparate(value, this.tokenSeparators);
        }
    }
    tokenSeparate(inputValue, tokenSeparators) {
        const includesSeparators = (str, separators) => {
            // tslint:disable-next-line:prefer-for-of
            for (let i = 0; i < separators.length; ++i) {
                if (str.lastIndexOf(separators[i]) > 0) {
                    return true;
                }
            }
            return false;
        };
        const splitBySeparators = (str, separators) => {
            const reg = new RegExp(`[${separators.join()}]`);
            const array = str.split(reg).filter(token => token);
            return [...new Set(array)];
        };
        if (inputValue &&
            inputValue.length &&
            tokenSeparators.length &&
            this.mode !== 'default' &&
            includesSeparators(inputValue, tokenSeparators)) {
            const listOfLabel = splitBySeparators(inputValue, tokenSeparators);
            this.tokenize.next(listOfLabel);
        }
    }
    clearInputValue() {
        if (this.nzSelectSearchComponent) {
            this.nzSelectSearchComponent.clearInputValue();
        }
    }
    focus() {
        if (this.nzSelectSearchComponent) {
            this.nzSelectSearchComponent.focus();
        }
    }
    blur() {
        if (this.nzSelectSearchComponent) {
            this.nzSelectSearchComponent.blur();
        }
    }
    trackValue(_index, option) {
        return option.nzValue;
    }
    onDeleteItem(item) {
        if (!this.disabled && !item.nzDisabled) {
            this.deleteItem.next(item);
        }
    }
    onAnimationEnd() {
        this.animationEnd.next();
    }
    ngOnChanges(changes) {
        const { listOfTopItem, maxTagCount, customTemplate, maxTagPlaceholder } = changes;
        if (listOfTopItem) {
            this.updateTemplateVariable();
        }
        if (listOfTopItem || maxTagCount || customTemplate || maxTagPlaceholder) {
            const listOfSlicedItem = this.listOfTopItem.slice(0, this.maxTagCount).map(o => {
                return {
                    nzLabel: o.nzLabel,
                    nzValue: o.nzValue,
                    nzDisabled: o.nzDisabled,
                    contentTemplateOutlet: this.customTemplate,
                    contentTemplateOutletContext: o
                };
            });
            if (this.listOfTopItem.length > this.maxTagCount) {
                const exceededLabel = `+ ${this.listOfTopItem.length - this.maxTagCount} ...`;
                const listOfSelectedValue = this.listOfTopItem.map(item => item.nzValue);
                const exceededItem = {
                    nzLabel: exceededLabel,
                    nzValue: '$$__nz_exceeded_item',
                    nzDisabled: true,
                    contentTemplateOutlet: this.maxTagPlaceholder,
                    contentTemplateOutletContext: listOfSelectedValue.slice(this.maxTagCount)
                };
                listOfSlicedItem.push(exceededItem);
            }
            this.listOfSlicedItem = listOfSlicedItem;
        }
    }
}
NzSelectTopControlComponent.decorators = [
    { type: Component, args: [{
                selector: 'nz-select-top-control',
                exportAs: 'nzSelectTopControl',
                preserveWhitespaces: false,
                animations: [zoomMotion],
                changeDetection: ChangeDetectionStrategy.OnPush,
                encapsulation: ViewEncapsulation.None,
                template: `
    <!--single mode-->
    <ng-container [ngSwitch]="mode">
      <ng-container *ngSwitchCase="'default'">
        <nz-select-search
          [disabled]="disabled"
          [value]="inputValue!"
          [showInput]="showSearch"
          [mirrorSync]="false"
          [autofocus]="autofocus"
          [focusTrigger]="open"
          (isComposingChange)="isComposingChange($event)"
          (valueChange)="onInputValueChange($event)"
        ></nz-select-search>
        <nz-select-item
          *ngIf="isShowSingleLabel"
          [deletable]="false"
          [disabled]="false"
          [removeIcon]="removeIcon"
          [label]="listOfTopItem[0].nzLabel"
          [contentTemplateOutlet]="customTemplate"
          [contentTemplateOutletContext]="listOfTopItem[0]"
        ></nz-select-item>
      </ng-container>
      <ng-container *ngSwitchDefault>
        <!--multiple or tags mode-->
        <nz-select-item
          *ngFor="let item of listOfSlicedItem; trackBy: trackValue"
          [@zoomMotion]
          [@.disabled]="noAnimation?.nzNoAnimation"
          [nzNoAnimation]="noAnimation?.nzNoAnimation"
          [removeIcon]="removeIcon"
          [label]="item.nzLabel"
          [disabled]="item.nzDisabled || disabled"
          [contentTemplateOutlet]="item.contentTemplateOutlet"
          [deletable]="true"
          [contentTemplateOutletContext]="item.contentTemplateOutletContext"
          (@zoomMotion.done)="onAnimationEnd()"
          (delete)="onDeleteItem(item.contentTemplateOutletContext)"
        ></nz-select-item>
        <nz-select-search
          [disabled]="disabled"
          [value]="inputValue!"
          [autofocus]="autofocus"
          [showInput]="true"
          [mirrorSync]="true"
          [focusTrigger]="open"
          (isComposingChange)="isComposingChange($event)"
          (valueChange)="onInputValueChange($event)"
        ></nz-select-search>
      </ng-container>
    </ng-container>
    <nz-select-placeholder *ngIf="isShowPlaceholder" [placeholder]="placeHolder"></nz-select-placeholder>
  `,
                host: {
                    '[class.ant-select-selector]': 'true',
                    '(keydown)': 'onHostKeydown($event)'
                }
            },] }
];
NzSelectTopControlComponent.ctorParameters = () => [
    { type: NzNoAnimationDirective, decorators: [{ type: Host }, { type: Optional }] }
];
NzSelectTopControlComponent.propDecorators = {
    showSearch: [{ type: Input }],
    placeHolder: [{ type: Input }],
    open: [{ type: Input }],
    maxTagCount: [{ type: Input }],
    autofocus: [{ type: Input }],
    disabled: [{ type: Input }],
    mode: [{ type: Input }],
    customTemplate: [{ type: Input }],
    maxTagPlaceholder: [{ type: Input }],
    removeIcon: [{ type: Input }],
    listOfTopItem: [{ type: Input }],
    tokenSeparators: [{ type: Input }],
    tokenize: [{ type: Output }],
    inputValueChange: [{ type: Output }],
    animationEnd: [{ type: Output }],
    deleteItem: [{ type: Output }],
    nzSelectSearchComponent: [{ type: ViewChild, args: [NzSelectSearchComponent,] }]
};
//# sourceMappingURL=data:application/json;base64,eyJ2ZXJzaW9uIjozLCJmaWxlIjoic2VsZWN0LXRvcC1jb250cm9sLmNvbXBvbmVudC5qcyIsInNvdXJjZVJvb3QiOiIvaG9tZS92c3RzL3dvcmsvMS9zL2NvbXBvbmVudHMvc2VsZWN0LyIsInNvdXJjZXMiOlsic2VsZWN0LXRvcC1jb250cm9sLmNvbXBvbmVudC50cyJdLCJuYW1lcyI6W10sIm1hcHBpbmdzIjoiQUFBQTs7O0dBR0c7QUFFSCxPQUFPLEVBQUUsU0FBUyxFQUFFLE1BQU0sdUJBQXVCLENBQUM7QUFDbEQsT0FBTyxFQUNMLHVCQUF1QixFQUN2QixTQUFTLEVBQ1QsWUFBWSxFQUNaLElBQUksRUFDSixLQUFLLEVBRUwsUUFBUSxFQUNSLE1BQU0sRUFHTixTQUFTLEVBQ1QsaUJBQWlCLEVBQ2xCLE1BQU0sZUFBZSxDQUFDO0FBQ3ZCLE9BQU8sRUFBRSxVQUFVLEVBQUUsTUFBTSw4QkFBOEIsQ0FBQztBQUMxRCxPQUFPLEVBQUUsc0JBQXNCLEVBQUUsTUFBTSxpQ0FBaUMsQ0FBQztBQUV6RSxPQUFPLEVBQUUsdUJBQXVCLEVBQUUsTUFBTSwyQkFBMkIsQ0FBQztBQXFFcEUsTUFBTSxPQUFPLDJCQUEyQjtJQStHdEMsWUFBdUMsV0FBb0M7UUFBcEMsZ0JBQVcsR0FBWCxXQUFXLENBQXlCO1FBOUdsRSxlQUFVLEdBQUcsS0FBSyxDQUFDO1FBQ25CLGdCQUFXLEdBQTJDLElBQUksQ0FBQztRQUMzRCxTQUFJLEdBQUcsS0FBSyxDQUFDO1FBQ2IsZ0JBQVcsR0FBVyxRQUFRLENBQUM7UUFDL0IsY0FBUyxHQUFHLEtBQUssQ0FBQztRQUNsQixhQUFRLEdBQUcsS0FBSyxDQUFDO1FBQ2pCLFNBQUksR0FBcUIsU0FBUyxDQUFDO1FBQ25DLG1CQUFjLEdBQTZELElBQUksQ0FBQztRQUNoRixzQkFBaUIsR0FBbUQsSUFBSSxDQUFDO1FBQ3pFLGVBQVUsR0FBa0MsSUFBSSxDQUFDO1FBQ2pELGtCQUFhLEdBQTRCLEVBQUUsQ0FBQztRQUM1QyxvQkFBZSxHQUFhLEVBQUUsQ0FBQztRQUNyQixhQUFRLEdB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QUksQ0FBQyxpQkFBaUI7b0JBQzdDLDRCQUE0QixFQUFFLG1CQUFtQixDQUFDLEtBQUssQ0FBQyxJQUFJLENBQUMsV0FBVyxDQUFDO2lCQUMxRSxDQUFDO2dCQUNGLGdCQUFnQixDQUFDLElBQUksQ0FBQyxZQUFZLENBQUMsQ0FBQzthQUNyQztZQUNELElBQUksQ0FBQyxnQkFBZ0IsR0FBRyxnQkFBZ0IsQ0FBQztTQUMxQztJQUNILENBQUM7OztZQWhORixTQUFTLFNBQUM7Z0JBQ1QsUUFBUSxFQUFFLHVCQUF1QjtnQkFDakMsUUFBUSxFQUFFLG9CQUFvQjtnQkFDOUIsbUJBQW1CLEVBQUUsS0FBSztnQkFDMUIsVUFBVSxFQUFFLENBQUMsVUFBVSxDQUFDO2dCQUN4QixlQUFlLEVBQUUsdUJBQXVCLENBQUMsTUFBTTtnQkFDL0MsYUFBYSxFQUFFLGlCQUFpQixDQUFDLElBQUk7Z0JBQ3JDLFFBQVEsRUFBRTs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7Ozs7R0FxRFQ7Z0JBQ0QsSUFBSSxFQUFFO29CQUNKLDZCQUE2QixFQUFFLE1BQU07b0JBQ3JDLFdBQVcsRUFBRSx1QkFBdUI7aUJBQ3JDO2FBQ0Y7OztZQXRFUSxzQkFBc0IsdUJBc0xoQixJQUFJLFlBQUksUUFBUTs7O3lCQTlHNUIsS0FBSzswQkFDTCxLQUFLO21CQUNMLEtBQUs7MEJBQ0wsS0FBSzt3QkFDTCxLQUFLO3VCQUNMLEtBQUs7bUJBQ0wsS0FBSzs2QkFDTCxLQUFLO2dDQUNMLEtBQUs7eUJBQ0wsS0FBSzs0QkFDTCxLQUFLOzhCQUNMLEtBQUs7dUJBQ0wsTUFBTTsrQkFDTixNQUFNOzJCQUNOLE1BQU07eUJBQ04sTUFBTTtzQ0FDTixTQUFTLFNBQUMsdUJBQXVCIiwic291cmNlc0NvbnRlbnQiOlsiLyoqXG4gKiBVc2Ugb2YgdGhpcyBzb3VyY2UgY29kZSBpcyBnb3Zlcm5lZCBieSBhbiBNSVQtc3R5bGUgbGljZW5zZSB0aGF0IGNhbiBiZVxuICogZm91bmQgaW4gdGhlIExJQ0VOU0UgZmlsZSBhdCBodHRwczovL2dpdGh1Yi5jb20vTkctWk9SUk8vbmctem9ycm8tYW50ZC9ibG9iL21hc3Rlci9MSUNFTlNFXG4gKi9cblxuaW1wb3J0IHsgQkFDS1NQQUNFIH0gZnJvbSAnQGFuZ3VsYXIvY2RrL2tleWNvZGVzJztcbmltcG9ydCB7XG4gIENoYW5nZURldGVjdGlvblN0cmF0ZWd5LFxuICBDb21wb25lbnQsXG4gIEV2ZW50RW1pdHRlcixcbiAgSG9zdCxcbiAgSW5wdXQsXG4gIE9uQ2hhbmdlcyxcbiAgT3B0aW9uYWwsXG4gIE91dHB1dCxcbiAgU2ltcGxlQ2hhbmdlcyxcbiAgVGVtcGxhdGVSZWYsXG4gIFZpZXdDaGlsZCxcbiAgVmlld0VuY2Fwc3VsYXRpb25cbn0gZnJvbSAnQGFuZ3VsYXIvY29yZSc7XG5pbXBvcnQgeyB6b29tTW90aW9uIH0gZnJvbSAnbmctem9ycm8tYW50ZC9jb3JlL2FuaW1hdGlvbic7XG5pbXBvcnQgeyBOek5vQW5pbWF0aW9uRGlyZWN0aXZlIH0gZnJvbSAnbmctem9ycm8tYW50ZC9jb3JlL25vLWFuaW1hdGlvbic7XG5pbXBvcnQgeyBOelNhZmVBbnkgfSBmcm9tICduZy16b3Jyby1hbnRkL2NvcmUvdHlwZXMnO1xuaW1wb3J0IHsgTnpTZWxlY3RTZWFyY2hDb21wb25lbnQgfSBmcm9tICcuL3NlbGVjdC1zZWFyY2guY29tcG9uZW50JztcbmltcG9ydCB7IE56U2VsZWN0SXRlbUludGVyZmFjZSwgTnpTZWxlY3RNb2RlVHlwZSwgTnpTZWxlY3RUb3BDb250cm9sSXRlbVR5cGUgfSBmcm9tICcuL3NlbGVjdC50eXBlcyc7XG5cbkBDb21wb25lbnQoe1xuICBzZWxlY3RvcjogJ256LXNlbGVjdC10b3AtY29udHJvbCcsXG4gIGV4cG9ydEFzOiAnbnpTZWxlY3RUb3BDb250cm9sJyxcbiAgcHJlc2VydmVXaGl0ZXNwYWNlczogZmFsc2UsXG4gIGFuaW1hdGlvbnM6IFt6b29tTW90aW9uXSxcbiAgY2hhbmdlRGV0ZWN0aW9uOiBDaGFuZ2VEZXRlY3Rpb25TdHJhdGVneS5PblB1c2gsXG4gIGVuY2Fwc3VsYXRpb246IFZpZXdFbmNhcHN1bGF0aW9uLk5vbmUsXG4gIHRlbXBsYXRlOiBgXG4gICAgPCEtLXNpbmdsZSBtb2RlLS0+XG4gICAgPG5nLWNvbnRhaW5lciBbbmdTd2l0Y2hdPVwibW9kZVwiPlxuICAgICAgPG5nLWNvbnRhaW5lciAqbmdTd2l0Y2hDYXNlPVwiJ2RlZmF1bHQnXCI+XG4gICAgICAgIDxuei1zZWxlY3Qtc2VhcmNoXG4gICAgICAgICAgW2Rpc2FibGVkXT1cImRpc2FibGVkXCJcbiAgICAgICAgICBbdmFsdWVdPVwiaW5wdXRWYWx1ZSFcIlxuICAgICAgICAgIFtzaG93SW5wdXRdPVwic2hvd1NlYXJjaFwiXG4gICAgICAgICAgW21pcnJvclN5bmNdPVwiZmFsc2VcIlxuICAgICAgICAgIFthdXRvZm9jdXNdPVwiYXV0b2ZvY3VzXCJcbiAgICAgICAgICBbZm9jdXNUcmlnZ2VyXT1cIm9wZW5cIlxuICAgICAgICAgIChpc0NvbXBvc2luZ0NoYW5nZSk9XCJpc0NvbXBvc2luZ0NoYW5nZSgkZXZlbnQpXCJcbiAgICAgICAgICAodmFsdWVDaGFuZ2UpPVwib25JbnB1dFZhbHVlQ2hhbmdlKCRldmVudClcIlxuICAgICAgICA+PC9uei1zZWxlY3Qtc2VhcmNoPlxuICAgICAgICA8bnotc2VsZWN0LWl0ZW1cbiAgICAgICAgICAqbmdJZj1cImlzU2hvd1NpbmdsZUxhYmVsXCJcbiAgICAgICAgICBbZGVsZXRhYmxlXT1cImZhbHNlXCJcbiAgICAgICAgICBbZGlzYWJsZWRdPVwiZmFsc2VcIlxuICAgICAgICAgIFtyZW1vdmVJY29uXT1cInJlbW92ZUljb25cIlxuICAgICAgICAgIFtsYWJlbF09XCJsaXN0T2ZUb3BJdGVtWzBdLm56TGFiZWxcIlxuICAgICAgICAgIFtjb250ZW50VGVtcGxhdGVPdXRsZXRdPVwiY3VzdG9tVGVtcGxhdGVcIlxuICAgICAgICAgIFtjb250ZW50VGVtcGxhdGVPdXRsZXRDb250ZXh0XT1cImxpc3RPZlRvcEl0ZW1bMF1cIlxuICAgICAgICA+PC9uei1zZWxlY3QtaXRlbT5cbiAgICAgIDwvbmctY29udGFpbmVyPlxuICAgICAgPG5nLWNvbnRhaW5lciAqbmdTd2l0Y2hEZWZhdWx0PlxuICAgICAgICA8IS0tbXVsdGlwbGUgb3IgdGFncyBtb2RlLS0+XG4gICAgICAgIDxuei1zZWxlY3QtaXRlbVxuICAgICAgICAgICpuZ0Zvcj1cImxldCBpdGVtIG9mIGxpc3RPZlNsaWNlZEl0ZW07IHRyYWNrQnk6IHRyYWNrVmFsdWVcIlxuICAgICAgICAgIFtAem9vbU1vdGlvbl1cbiAgICAgICAgICBbQC5kaXNhYmxlZF09XCJub0FuaW1hdGlvbj8ubnpOb0FuaW1hdGlvblwiXG4gICAgICAgICAgW256Tm9BbmltYXRpb25dPVwibm9BbmltYXRpb24/Lm56Tm9BbmltYXRpb25cIlxuICAgICAgICAgIFtyZW1vdmVJY29uXT1cInJlbW92ZUljb25cIlxuICAgICAgICAgIFtsYWJlbF09XCJpdGVtLm56TGFiZWxcIlxuICAgICAgICAgIFtkaXNhYmxlZF09XCJpdGVtLm56RGlzYWJsZWQgfHwgZGlzYWJsZWRcIlxuICAgICAgICAgIFtjb250ZW50VGVtcGxhdGVPdXRsZXRdPVwiaXRlbS5jb250ZW50VGVtcGxhdGVPdXRsZXRcIlxuICAgICAgICAgIFtkZWxldGFibGVdPVwidHJ1ZVwiXG4gICAgICAgICAgW2NvbnRlbnRUZW1wbGF0ZU91dGxldENvbnRleHRdPVwiaXRlbS5jb250ZW50VGVtcGxhdGVPdXRsZXRDb250ZXh0XCJcbiAgICAgICAgICAoQHpvb21Nb3Rpb24uZG9uZSk9XCJvbkFuaW1hdGlvbkVuZCgpXCJcbiAgICAgICAgICAoZGVsZXRlKT1cIm9uRGVsZXRlSXRlbShpdGVtLmNvbnRlbnRUZW1wbGF0ZU91dGxldENvbnRleHQpXCJcbiAgICAgICAgPjwvbnotc2VsZWN0LWl0ZW0+XG4gICAgICAgIDxuei1zZWxlY3Qtc2VhcmNoXG4gICAgICAgICAgW2Rpc2FibGVkXT1cImRpc2FibGVkXCJcbiAgICAgICAgICBbdmFsdWVdPVwiaW5wdXRWYWx1ZSFcIlxuICAgICAgICAgIFthdXRvZm9jdXNdPVwiYXV0b2ZvY3VzXCJcbiAgICAgICAgICBbc2hvd0lucHV0XT1cInRydWVcIlxuICAgICAgICAgIFttaXJyb3JTeW5jXT1cInRydWVcIlxuICAgICAgICAgIFtmb2N1c1RyaWdnZXJdPVwib3BlblwiXG4gICAgICAgICAgKGlzQ29tcG9zaW5nQ2hhbmdlKT1cImlzQ29tcG9zaW5nQ2hhbmdlKCRldmVudClcIlxuICAgICAgICAgICh2YWx1ZUNoYW5nZSk9XCJvbklucHV0VmFsdWVDaGFuZ2UoJGV2ZW50KVwiXG4gICAgICAgID48L256LXNlbGVjdC1zZWFyY2g+XG4gICAgICA8L25nLWNvbnRhaW5lcj5cbiAgICA8L25nLWNvbnRhaW5lcj5cbiAgICA8bnotc2VsZWN0LXBsYWNlaG9sZGVyICpuZ0lmPVwiaXNTaG93UGxhY2Vob2xkZXJcIiBbcGxhY2Vob2xkZXJdPVwicGxhY2VIb2xkZXJcIj48L256LXNlbGVjdC1wbGFjZWhvbGRlcj5cbiAgYCxcbiAgaG9zdDoge1xuICAgICdbY2xhc3MuYW50LXNlbGVjdC1zZWxlY3Rvcl0nOiAndHJ1ZScsXG4gICAgJyhrZXlkb3duKSc6ICdvbkhvc3RLZXlkb3duKCRldmVudCknXG4gIH1cbn0pXG5leHBvcnQgY2xhc3MgTnpTZWxlY3RUb3BDb250cm9sQ29tcG9uZW50IGltcGxlbWVudHMgT25DaGFuZ2VzIHtcbiAgQElucHV0KCkgc2hvd1NlYXJjaCA9IGZhbHNlO1xuICBASW5wdXQoKSBwbGFjZUhvbGRlcjogc3RyaW5nIHwgVGVtcGxhdGVSZWY8TnpTYWZlQW55PiB8IG51bGwgPSBudWxsO1xuICBASW5wdXQoKSBvcGVuID0gZmFsc2U7XG4gIEBJbnB1dCgpIG1heFRhZ0NvdW50OiBudW1iZXIgPSBJbmZpbml0eTtcbiAgQElucHV0KCkgYXV0b2ZvY3VzID0gZmFsc2U7XG4gIEBJbnB1dCgpIGRpc2FibGVkID0gZmFsc2U7XG4gIEBJbnB1dCgpIG1vZGU6IE56U2VsZWN0TW9kZVR5cGUgPSAnZGVmYXVsdCc7XG4gIEBJbnB1dCgpIGN1c3RvbVRlbXBsYXRlOiBUZW1wbGF0ZVJlZjx7ICRpbXBsaWNpdDogTnpTZWxlY3RJdGVtSW50ZXJmYWNlIH0+IHwgbnVsbCA9IG51bGw7XG4gIEBJbnB1dCgpIG1heFRhZ1BsYWNlaG9sZGVyOiBUZW1wbGF0ZVJlZjx7ICRpbXBsaWNpdDogTnpTYWZlQW55W10gfT4gfCBudWxsID0gbnVsbDtcbiAgQElucHV0KCkgcmVtb3ZlSWNvbjogVGVtcGxhdGVSZWY8TnpTYWZlQW55PiB8IG51bGwgPSBudWxsO1xuICBASW5wdXQoKSBsaXN0T2ZUb3BJdGVtOiBOelNlbGVjdEl0ZW1JbnRlcmZhY2VbXSA9IFtdO1xuICBASW5wdXQoKSB0b2tlblNlcGFyYXRvcnM6IHN0cmluZ1tdID0gW107XG4gIEBPdXRwdXQoKSByZWFkb25seSB0b2tlbml6ZSA9IG5ldyBFdmVudEVtaXR0ZXI8c3RyaW5nW10+KCk7XG4gIEBPdXRwdXQoKSByZWFkb25seSBpbnB1dFZhbHVlQ2hhbmdlID0gbmV3IEV2ZW50RW1pdHRlcjxzdHJpbmc+KCk7XG4gIEBPdXRwdXQoKSByZWFkb25seSBhbmltYXRpb25FbmQgPSBuZXcgRXZlbnRFbWl0dGVyPHZvaWQ+KCk7XG4gIEBPdXRwdXQoKSByZWFkb25seSBkZWxldGVJdGVtID0gbmV3IEV2ZW50RW1pdHRlcjxOelNlbGVjdEl0ZW1JbnRlcmZhY2U+KCk7XG4gIEBWaWV3Q2hpbGQoTnpTZWxlY3RTZWFyY2hDb21wb25lbnQpIG56U2VsZWN0U2VhcmNoQ29tcG9uZW50ITogTnpTZWxlY3RTZWFyY2hDb21wb25lbnQ7XG4gIGxpc3RPZlNsaWNlZEl0ZW06IE56U2VsZWN0VG9wQ29udHJvbEl0ZW1UeXBlW10gPSBbXTtcbiAgaXNTaG93UGxhY2Vob2xkZXIgPSB0cnVlO1xuICBpc1Nob3dTaW5nbGVMYWJlbCA9IGZhbHNlO1xuICBpc0NvbXBvc2luZyA9IGZhbHNlO1xuICBpbnB1dFZhbHVlOiBzdHJpbmcgfCBudWxsID0gbnVsbDtcblxuICBvbkhvc3RLZXlkb3duKGU6IEtleWJvYXJkRXZlbnQpOiB2b2lkIHtcbiAgICBjb25zdCBpbnB1dFZhbHVlID0gKGUudGFyZ2V0IGFzIEhUTUxJbnB1dEVsZW1lbnQpLnZhbHVlO1xuICAgIGlmIChlLmtleUNvZGUgPT09IEJBQ0tTUEFDRSAmJiB0aGlzLm1vZGUgIT09ICdkZWZhdWx0JyAmJiAhaW5wdXRWYWx1ZSAmJiB0aGlzLmxpc3RPZlRvcEl0ZW0ubGVuZ3RoID4gMCkge1xuICAgICAgZS5wcmV2ZW50RGVmYXVsdCgpO1xuICAgICAgdGhpcy5vbkRlbGV0ZUl0ZW0odGhpcy5saXN0T2ZUb3BJdGVtW3RoaXMubGlzdE9mVG9wSXRlbS5sZW5ndGggLSAxXSk7XG4gICAgfVxuICB9XG5cbiAgdXBkYXRlVGVtcGxhdGVWYXJpYWJsZSgpOiB2b2lkIHtcbiAgICBjb25zdCBpc1NlbGVjdGVkVmFsdWVFbXB0eSA9IHRoaXMubGlzdE9mVG9wSXRlbS5sZW5ndGggPT09IDA7XG4gICAgdGhpcy5pc1Nob3dQbGFjZWhvbGRlciA9IGlzU2VsZWN0ZWRWYWx1ZUVtcHR5ICYmICF0aGlzLmlzQ29tcG9zaW5nICYmICF0aGlzLmlucHV0VmFsdWU7XG4gICAgdGhpcy5pc1Nob3dTaW5nbGVMYWJlbCA9ICFpc1NlbGVjdGVkVmFsdWVFbXB0eSAmJiAhdGhpcy5pc0NvbXBvc2luZyAmJiAhdGhpcy5pbnB1dFZhbHVlO1xuICB9XG5cbiAgaXNDb21wb3NpbmdDaGFuZ2UoaXNDb21wb3Npbmc6IGJvb2xlYW4pOiB2b2lkIHtcbiAgICB0aGlzLmlzQ29tcG9zaW5nID0gaXNDb21wb3Npbmc7XG4gICAgdGhpcy51cGRhdGVUZW1wbGF0ZVZhcmlhYmxlKCk7XG4gIH1cblxuICBvbklucHV0VmFsdWVDaGFuZ2UodmFsdWU6IHN0cmluZyk6IHZvaWQge1xuICAgIGlmICh2YWx1ZSAhPT0gdGhpcy5pbnB1dFZhbHVlKSB7XG4gICAgICB0aGlzLmlucHV0VmFsdWUgPSB2YWx1ZTtcbiAgICAgIHRoaXMudXBkYXRlVGVtcGxhdGVWYXJpYWJsZSgpO1xuICAgICAgdGhpcy5pbnB1dFZhbHVlQ2hhbmdlLmVtaXQodmFsdWUpO1xuICAgICAgdGhpcy50b2tlblNlcGFyYXRlKHZhbHVlLCB0aGlzLnRva2VuU2VwYXJhdG9ycyk7XG4gICAgfVxuICB9XG5cbiAgdG9rZW5TZXBhcmF0ZShpbnB1dFZhbHVlOiBzdHJpbmcsIHRva2VuU2VwYXJhdG9yczogc3RyaW5nW10pOiB2b2lkIHtcbiAgICBjb25zdCBpbmNsdWRlc1NlcGFyYXRvcnMgPSAoc3RyOiBzdHJpbmcgfCBzdHJpbmdbXSwgc2VwYXJhdG9yczogc3RyaW5nW10pOiBib29sZWFuID0+IHtcbiAgICAgIC8vIHRzbGludDpkaXNhYmxlLW5leHQtbGluZTpwcmVmZXItZm9yLW9mXG4gICAgICBmb3IgKGxldCBpID0gMDsgaSA8IHNlcGFyYXRvcnMubGVuZ3RoOyArK2kpIHtcbiAgICAgICAgaWYgKHN0ci5sYXN0SW5kZXhPZihzZXBhcmF0b3JzW2ldKSA+IDApIHtcbiAgICAgICAgICByZXR1cm4gdHJ1ZTtcbiAgICAgICAgfVxuICAgICAgfVxuICAgICAgcmV0dXJuIGZhbHNlO1xuICAgIH07XG4gICAgY29uc3Qgc3BsaXRCeVNlcGFyYXRvcnMgPSAoc3RyOiBzdHJpbmcgfCBzdHJpbmdbXSwgc2VwYXJhdG9yczogc3RyaW5nW10pOiBzdHJpbmdbXSA9PiB7XG4gICAgICBjb25zdCByZWcgPSBuZXcgUmVnRXhwKGBbJHtzZXBhcmF0b3JzLmpvaW4oKX1dYCk7XG4gICAgICBjb25zdCBhcnJheSA9IChzdHIgYXMgc3RyaW5nKS5zcGxpdChyZWcpLmZpbHRlcih0b2tlbiA9PiB0b2tlbik7XG4gICAgICByZXR1cm4gWy4uLm5ldyBTZXQoYXJyYXkpXTtcbiAgICB9O1xuICAgIGlmIChcbiAgICAgIGlucHV0VmFsdWUgJiZcbiAgICAgIGlucHV0VmFsdWUubGVuZ3RoICYmXG4gICAgICB0b2tlblNlcGFyYXRvcnMubGVuZ3RoICYmXG4gICAgICB0aGlzLm1vZGUgIT09ICdkZWZhdWx0JyAmJlxuICAgICAgaW5jbHVkZXNTZXBhcmF0b3JzKGlucHV0VmFsdWUsIHRva2VuU2VwYXJhdG9ycylcbiAgICApIHtcbiAgICAgIGNvbnN0IGxpc3RPZkxhYmVsID0gc3BsaXRCeVNlcGFyYXRvcnMoaW5wdXRWYWx1ZSwgdG9rZW5TZXBhcmF0b3JzKTtcbiAgICAgIHRoaXMudG9rZW5pemUubmV4dChsaXN0T2ZMYWJlbCk7XG4gICAgfVxuICB9XG5cbiAgY2xlYXJJbnB1dFZhbHVlKCk6IHZvaWQge1xuICAgIGlmICh0aGlzLm56U2VsZWN0U2VhcmNoQ29tcG9uZW50KSB7XG4gICAgICB0aGlzLm56U2VsZWN0U2VhcmNoQ29tcG9uZW50LmNsZWFySW5wdXRWYWx1ZSgpO1xuICAgIH1cbiAgfVxuXG4gIGZvY3VzKCk6IHZvaWQge1xuICAgIGlmICh0aGlzLm56U2VsZWN0U2VhcmNoQ29tcG9uZW50KSB7XG4gICAgICB0aGlzLm56U2VsZWN0U2VhcmNoQ29tcG9uZW50LmZvY3VzKCk7XG4gICAgfVxuICB9XG5cbiAgYmx1cigpOiB2b2lkIHtcbiAgICBpZiAodGhpcy5uelNlbGVjdFNlYXJjaENvbXBvbmVudCkge1xuICAgICAgdGhpcy5uelNlbGVjdFNlYXJjaENvbXBvbmVudC5ibHVyKCk7XG4gICAgfVxuICB9XG5cbiAgdHJhY2tWYWx1ZShfaW5kZXg6IG51bWJlciwgb3B0aW9uOiBOelNlbGVjdFRvcENvbnRyb2xJdGVtVHlwZSk6IE56U2FmZUFueSB7XG4gICAgcmV0dXJuIG9wdGlvbi5uelZhbHVlO1xuICB9XG5cbiAgb25EZWxldGVJdGVtKGl0ZW06IE56U2VsZWN0SXRlbUludGVyZmFjZSk6IHZvaWQge1xuICAgIGlmICghdGhpcy5kaXNhYmxlZCAmJiAhaXRlbS5uekRpc2FibGVkKSB7XG4gICAgICB0aGlzLmRlbGV0ZUl0ZW0ubmV4dChpdGVtKTtcbiAgICB9XG4gIH1cblxuICBvbkFuaW1hdGlvbkVuZCgpOiB2b2lkIHtcbiAgICB0aGlzLmFuaW1hdGlvbkVuZC5uZXh0KCk7XG4gIH1cblxuICBjb25zdHJ1Y3RvcihASG9zdCgpIEBPcHRpb25hbCgpIHB1YmxpYyBub0FuaW1hdGlvbj86IE56Tm9BbmltYXRpb25EaXJlY3RpdmUpIHt9XG5cbiAgbmdPbkNoYW5nZXMoY2hhbmdlczogU2ltcGxlQ2hhbmdlcyk6IHZvaWQge1xuICAgIGNvbnN0IHsgbGlzdE9mVG9wSXRlbSwgbWF4VGFnQ291bnQsIGN1c3RvbVRlbXBsYXRlLCBtYXhUYWdQbGFjZWhvbGRlciB9ID0gY2hhbmdlcztcbiAgICBpZiAobGlzdE9mVG9wSXRlbSkge1xuICAgICAgdGhpcy51cGRhdGVUZW1wbGF0ZVZhcmlhYmxlKCk7XG4gICAgfVxuICAgIGlmIChsaXN0T2ZUb3BJdGVtIHx8IG1heFRhZ0NvdW50IHx8IGN1c3RvbVRlbXBsYXRlIHx8IG1heFRhZ1BsYWNlaG9sZGVyKSB7XG4gICAgICBjb25zdCBsaXN0T2ZTbGljZWRJdGVtOiBOelNlbGVjdFRvcENvbnRyb2xJdGVtVHlwZVtdID0gdGhpcy5saXN0T2ZUb3BJdGVtLnNsaWNlKDAsIHRoaXMubWF4VGFnQ291bnQpLm1hcChvID0+IHtcbiAgICAgICAgcmV0dXJuIHtcbiAgICAgICAgICBuekxhYmVsOiBvLm56TGFiZWwsXG4gICAgICAgICAgbnpWYWx1ZTogby5uelZhbHVlLFxuICAgICAgICAgIG56RGlzYWJsZWQ6IG8ubnpEaXNhYmxlZCxcbiAgICAgICAgICBjb250ZW50VGVtcGxhdGVPdXRsZXQ6IHRoaXMuY3VzdG9tVGVtcGxhdGUsXG4gICAgICAgICAgY29udGVudFRlbXBsYXRlT3V0bGV0Q29udGV4dDogb1xuICAgICAgICB9O1xuICAgICAgfSk7XG4gICAgICBpZiAodGhpcy5saXN0T2ZUb3BJdGVtLmxlbmd0aCA+IHRoaXMubWF4VGFnQ291bnQpIHtcbiAgICAgICAgY29uc3QgZXhjZWVkZWRMYWJlbCA9IGArICR7dGhpcy5saXN0T2ZUb3BJdGVtLmxlbmd0aCAtIHRoaXMubWF4VGFnQ291bnR9IC4uLmA7XG4gICAgICAgIGNvbnN0IGxpc3RPZlNlbGVjdGVkVmFsdWUgPSB0aGlzLmxpc3RPZlRvcEl0ZW0ubWFwKGl0ZW0gPT4gaXRlbS5uelZhbHVlKTtcbiAgICAgICAgY29uc3QgZXhjZWVkZWRJdGVtID0ge1xuICAgICAgICAgIG56TGFiZWw6IGV4Y2VlZGVkTGFiZWwsXG4gICAgICAgICAgbnpWYWx1ZTogJyQkX19uel9leGNlZWRlZF9pdGVtJyxcbiAgICAgICAgICBuekRpc2FibGVkOiB0cnVlLFxuICAgICAgICAgIGNvbnRlbnRUZW1wbGF0ZU91dGxldDogdGhpcy5tYXhUYWdQbGFjZWhvbGRlcixcbiAgICAgICAgICBjb250ZW50VGVtcGxhdGVPdXRsZXRDb250ZXh0OiBsaXN0T2ZTZWxlY3RlZFZhbHVlLnNsaWNlKHRoaXMubWF4VGFnQ291bnQpXG4gICAgICAgIH07XG4gICAgICAgIGxpc3RPZlNsaWNlZEl0ZW0ucHVzaChleGNlZWRlZEl0ZW0pO1xuICAgICAgfVxuICAgICAgdGhpcy5saXN0T2ZTbGljZWRJdGVtID0gbGlzdE9mU2xpY2VkSXRlbTtcbiAgICB9XG4gIH1cbn1cbiJdfQ==