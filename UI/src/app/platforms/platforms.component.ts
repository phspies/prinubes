import { Component, OnInit, ViewChild } from '@angular/core';
import { FormControl } from '@angular/forms';
@Component({
  selector: 'platforms-tabs',
  templateUrl: './platforms.component.html',
  styleUrls: ['./platforms.component.scss'],
})
export class PlatformComponent implements OnInit {

  ngOnInit() {}
  tabs = ['Compute Platforms', 'Network Platforms', 'Load Balancer Platdforms'];
  selected = new FormControl(0);
  addTab(selectAfterAdding: boolean) {
    this.tabs.push('New');
    if (selectAfterAdding) {
      this.selected.setValue(this.tabs.length - 1);
    }
  }
  removeTab(index: number) {
    this.tabs.splice(index, 1);
  }


}
